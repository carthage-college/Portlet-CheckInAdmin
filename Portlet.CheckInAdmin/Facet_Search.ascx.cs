using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Jenzabar.Common;
using Jenzabar.Common.Globalization;
using Jenzabar.Common.Web.UI.Controls;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using Portlet.CheckInStudent;
using CUS.OdbcConnectionClass3;
//Export to Excel
using System.IO;
using System.Drawing;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;

namespace Portlet.CheckInAdmin
{
    public partial class Facet_Search : PortletViewBase
    {
        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected void Page_Init(object sender, EventArgs e)
        {
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;

            this.panelResultCount.Visible = this.btnExportExcel.Visible = false;

            #region Load Office Table

            if (IsFirstLoad)
            {
                #region Original Table Load

                //DataTable dtOffices = null;
                //Exception exOffices = null;
                //string sqlOffices = "EXECUTE CUS_spCheckIn_Offices";

                //try
                //{
                //    dtOffices = spConn.ConnectToERP(sqlOffices, ref exOffices);
                //    if (exOffices != null) { throw exOffices; }
                //    if (dtOffices != null && dtOffices.Rows.Count > 0)
                //    {
                //        foreach (DataRow drOffice in dtOffices.Rows)
                //        {
                //            string officeName = drOffice["OfficeName"].ToString();
                //            tblOffices.Rows.Add(OfficeRow(officeName, drOffice["OfficeID"].ToString()));

                //            DataTable dtTasks = ciHelper.GetTasks();
                //            List<string> taskNames = dtTasks.AsEnumerable().Where(tn => tn.Field<string>("OfficeName") == officeName).Select(tn => tn.Field<string>("ViewColumn")).ToList();
                //            Dictionary<string, string> tasks = dtTasks.AsEnumerable()
                //                .Where(task => task.Field<string>("OfficeName") == officeName)
                //                .ToDictionary(task => task.Field<string>("TaskName"), task => task.Field<string>("ViewColumn"));

                //            foreach(KeyValuePair<string, string> task in tasks)
                //            {
                //                tblOffices.Rows.Add(TaskRow(task.Key, drOffice["OfficeID"].ToString(), task.Value));
                //            }
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An error occurred while retrieving office information", ex));
                //}
                //finally
                //{
                //    if (spConn.IsNotClosed()) { spConn.Close(); }
                //}

                #endregion

                #region New/Updated Table Load

                DataTable dtOffice = null;
                try
                {
                    dtOffice = ciHelper.GetOfficeAndTask();

                    string currentOffice = "";
                    foreach (DataRow drOffice in dtOffice.Rows)
                    {
                        string officeName = drOffice["OfficeName"].ToString(),
                            officeID = drOffice["OfficeID"].ToString();

                        //If the office has changed from the last iteration of the loop, create a new header row
                        if (currentOffice != officeName)
                        {
                            currentOffice = officeName;
                            tblOffices.Rows.Add(OfficeRow(currentOffice, officeID));
                        }

                        tblOffices.Rows.Add(TaskRow(drOffice["TaskName"].ToString(), officeID, drOffice["ViewColumn"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while loading office/task table", ex));
                }

                #endregion

                #region Load Dropdowns

                DataTable dtAthletics = ciHelper.GetAthletics();
                this.lbAthletics.DataSource = dtAthletics;
                this.lbAthletics.DataTextField = "involve_text";
                this.lbAthletics.DataValueField = "involve_code";
                this.lbAthletics.DataBind();

                #endregion
            }

            #endregion
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region Row and Control creation

        private TableRow OfficeRow(string officeName, string officeID)
        {
            return BuildRow(officeName, officeID, "Office");
        }

        private TableRow TaskRow(string taskName, string officeID, string viewColumn)
        {
            return BuildRow(taskName, officeID, "Task", viewColumn);
        }

        private TableRow BuildRow(string displayName, string officeID, string objType, string viewColumn = null)
        {
            viewColumn = String.IsNullOrWhiteSpace(viewColumn) ? displayName : viewColumn;

            TableRow tr = new TableRow();
            tr.CssClass = String.Format("radioRow{0}", objType);

            TableCell td = new TableCell();
            td.CssClass = String.Format("{0}Name", tr.CssClass);
            td.Text = displayName;
            tr.Cells.Add(td);

            List<string> statuses = new List<string>()
            {
                CheckInTaskStatus.Yes.ToDescriptionString(),
                CheckInTaskStatus.No.ToDescriptionString(),
                CheckInTaskStatus.Pending.ToDescriptionString(),
                CheckInTaskStatus.Waived.ToDescriptionString(),
                String.Format("{0}{1}", CheckInTaskStatus.Yes.ToDescriptionString(), CheckInTaskStatus.Waived.ToDescriptionString()),
                String.Format("{0}{1}", CheckInTaskStatus.No.ToDescriptionString(), CheckInTaskStatus.Pending.ToDescriptionString()),
                "*"
            };

            foreach (string status in statuses)
            {
                TableCell tdRadio = new TableCell();
                tdRadio.CssClass = String.Format("{0}Status", tr.CssClass);
                switch (objType.ToUpper())
                {
                    case "TASK":
                        tdRadio.Controls.Add(BuildTaskRadio(displayName, officeID, status, viewColumn));
                        break;
                    case "OFFICE":
                        tdRadio.Controls.Add(BuildOfficeRadio(displayName, officeID, status));
                        break;
                }
                tr.Cells.Add(tdRadio);
            }

            return tr;
        }

        private RadioButton BuildOfficeRadio(string officeName, string officeID, string status)
        {
            string officeNoSpace = officeName.Replace(" ", "");
            return BuildRadio("radioOffice", String.Format("Office{0}", officeNoSpace), "office", officeID, status);
        }

        private RadioButton BuildTaskRadio(string taskName, string officeID, string status, string viewColumn)
        {
            return BuildRadio("radioTask", String.Format("Task{0}", viewColumn), "task", officeID, status);
        }

        private RadioButton BuildRadio(string cssClass, string groupName, string control, string officeID, string status)
        {
            RadioButton rb = new RadioButton();
            rb.CssClass = cssClass;
            rb.ClientIDMode = ClientIDMode.Static;
            rb.GroupName = groupName;
            rb.ID = String.Format("radio{0}_{1}", groupName, status);
            rb.Attributes.Add("data-control", control);
            rb.Attributes.Add("data-office", officeID);
            rb.Attributes.Add("data-status", status);
            if (status == "*") { rb.Checked = true; }
            return rb;
        }

        #endregion

        private List<RadioButton> GetRadioGroup(Control ctrl, string groupName, List<RadioButton> results = null)
        {
            foreach (Control child in ctrl.Controls)
            {
                if (child.GetType() == typeof(RadioButton) && ((RadioButton)child).GroupName == groupName)
                {
                    if (results == null) { results = new List<RadioButton>() { }; }
                    results.Add((RadioButton)child);
                }
                else
                {
                    results = GetRadioGroup(child, groupName, results);
                }
            }
            return results;
        }

        private RadioButton getSelectedRadio(ControlCollection collection, string groupName)
        {
            RadioButton val = null;
            foreach (Control ctrl in collection)
            {
                if (ctrl.Controls.Count != 0)
                {
                    if (val == null)
                    {
                        val = getSelectedRadio(ctrl.Controls, groupName);
                    }
                }

                //if (ctrl.GetType() == typeof(RadioButton))
                if(ctrl.ToString() == "System.Web.UI.WebControls.RadioButton")
                {
                    RadioButton rb = (RadioButton)ctrl;
                    if (rb.GroupName == groupName && rb.Checked)
                    {
                        return rb;
                    }
                }
            }
            return val;
        }

        #region Event Handlers

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DataTable dtResults = GetSearchResults();

            var mstream = new MemoryStream();
            var sw = new StreamWriter(mstream);
            var dgResults = ciHelper.CreateDataGrid();

            ciHelper.ConfigureDataGrid(ref dgResults, dtResults, true, false, true, 5, "");

            dgResults.DataSource = dtResults;
            dgResults.DataBind();

            var stringWrite = new StringWriter();
            var htmlWrite = new HtmlTextWriter(stringWrite);
            dgResults.RenderControl(htmlWrite);

            htmlWrite.Flush();

            sw.WriteLine(stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", ""));

            sw.Flush();
            sw.Close();

            byte[] byteArray = mstream.ToArray();

            mstream.Flush();
            mstream.Close();

            Response.Clear();
            Response.AddHeader("Content-Type", "application/vnd.ms-excel");
            Response.AddHeader("Content-Disposition", "attachment; filename=ExportedData.xls");
            Response.AddHeader("Content-Length", byteArray.Length.ToString(CultureInfo.InvariantCulture));
            Response.ContentType = "application/octet-stream";
            Response.BinaryWrite(byteArray);
            Response.End();
        }

        protected void aNameSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.NextScreen("Search_Student");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtResults = GetSearchResults();
                this.dgResults.DataSource = dtResults;
                this.dgResults.DataBind();

                //Update the recordcount
                this.panelResultCount.Visible = this.btnExportExcel.Visible = true;
                this.ltlResultCount.Text = dtResults.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                ciHelper.FormatException("Error while retrieving faceted search results in button click event", ex, null, true);
            }
        }

        #endregion

        protected DataTable GetSearchResults()
        {
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtResults = new DataTable();
            Exception exResults = null;

            #region Dynamically build search SQL

            //Get the offices and task which are active for the current year/session
            DataTable dtOfficeTask = ciHelper.GetOfficeAndTask();
            string sqlSelect = "", sqlFrom = "", sqlWhere = "";

            //Loop through each task
            foreach (DataRow dr in dtOfficeTask.Rows)
            {
                string viewColumn = dr["ViewColumn"].ToString();
                string tableAlias = viewColumn.Replace("_", "");

                //Using ToTitleCase() to capitalize each word in the column alias
                TextInfo textinfo = new CultureInfo("en-US", false).TextInfo;
                string columnAlias = textinfo.ToTitleCase(viewColumn.Replace('_', ' '));

                //Build SELECT portion of SQL statment
                sqlSelect = String.Format("{0}, {1}.TaskStatus AS '{2}'", sqlSelect, tableAlias, columnAlias);

                //Build JOINS for SQL statement
                sqlFrom = String.Format(@"{0}
                            LEFT JOIN   CI_StudentProgress  {1} ON  U.ID        =   {1}.UserID
                                                                AND {1}.TaskID  =   (SELECT TaskID FROM CI_OfficeTask WHERE ViewColumn = '{2}')
                                                                AND {1}.Yr      =   {3}
                                                                AND {1}.Sess    =   '{4}'
                ", sqlFrom, tableAlias, viewColumn, helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

                //Get the collection of radio buttons which correspond to the current task
                List<RadioButton> radioForTask = GetRadioGroup(tblOffices, String.Format("Task{0}", viewColumn));

                //Was a radio button other than "Any" selected for this task?
                RadioButton selectedRadio = radioForTask.FirstOrDefault(rb => rb.Checked == true && !rb.ID.EndsWith("*"));

                if (radioForTask.Contains(selectedRadio))
                {
                    //Because some columns address multiple statuses (Y/W, N/P, etc), turn each status into an item in a list and format it for the SQL statement
                    string status = "";
                    List<string> statusList = selectedRadio.ID.Split('_').Last().Select(chr => chr.ToString()).ToList();
                    foreach (string stat in statusList)
                    {
                        status = String.Format("{0}{1}'{2}'", status, String.IsNullOrWhiteSpace(status) ? "" : ",", stat);
                    }

                    sqlWhere = String.Format("{0} AND {1}.TaskStatus IN ({2})", sqlWhere, tableAlias, status);
                }
            }

            #endregion

            string sqlResults = String.Format(@"
                SELECT
                    CAST(CAST(U.HostID AS INT) AS VARCHAR(10)) AS HostID, U.LastName AS 'Last Name', U.FirstName AS 'First Name', U.Email,
                    '' AS 'Admit Year', '' AS 'Admit Term', '' AS ClassCode, '' AS AcademicStanding{0}
                FROM
                    CI_StudentMetaData  SMD INNER JOIN  FWK_User    U   ON  SMD.UserID  =   U.ID
                                            {1}
                WHERE
                    SMD.ActiveYear      =   {3}
                AND
                    SMD.ActiveSession   =   '{4}'
                AND
                    SMD.IsActive        =   1
                {2}
                ORDER BY
                    U.LastName, U.FirstName, U.Email
            ", sqlSelect, sqlFrom, sqlWhere, helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            if (PortalUser.Current.IsSiteAdmin)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, sqlResults);
            }

            try
            {
                dtResults = jicsConn.ConnectToERP(sqlResults, ref exResults);
                if (exResults != null) { throw exResults; }

                ////////////////////////////////////////////////////////////////////
                #region Faceted Search - Standing

                if (!String.IsNullOrWhiteSpace(this.ddlStanding.SelectedValue))
                {
                    OdbcConnectionClass3 cxConn = helper.CONNECTION_CX_LIVE;
                    DataTable dtStanding = null;
                    Exception exStanding = null;
                    string sqlStanding = String.Format(@"
                        SELECT
	                        TRIM(host_id) AS id
                        FROM
	                        jenzcst_rec
                        WHERE
	                        status_code	{0} IN ('PFF','PTR')
                        GROUP BY
                            id
                    ", (this.ddlStanding.SelectedValue == "N" ? "NOT" : ""));

                    try
                    {
                        dtStanding = cxConn.ConnectToERP(sqlStanding, ref exStanding);
                        if (exStanding != null) { throw exStanding; }
                        if (dtStanding != null && dtStanding.Rows.Count > 0)
                        {
                            List<string> standingIDs = dtStanding.AsEnumerable().Select(standing => standing.Field<string>("id")).ToList();
                            var filteredRows = from row in dtResults.AsEnumerable()
                                               where standingIDs.Contains(row.Field<string>("HostID"))
                                               select row;
                            dtResults = filteredRows == null || filteredRows.Count() == 0 ? new DataTable() : filteredRows.CopyToDataTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while filtering facet search based on standing", ex, null, true));
                    }
                    finally
                    {
                        if (cxConn.IsNotClosed()) { cxConn.Close(); }
                    }
                }

                #endregion

                #region Faceted Search - Athletics

                List<ListItem> selectedSports = lbAthletics.Items.Cast<ListItem>().Where(item => item.Selected == true).ToList();
                if (selectedSports.Count > 0)
                {
                    string athleticsList = String.Format("'{0}'", String.Join("','", selectedSports.Select(li => li.Value).ToList()));

                    OdbcConnectionClass3 cxConn = helper.CONNECTION_CX_LIVE;
                    DataTable dtAthletics = null;
                    Exception exAthletics = null;

                    try
                    {
                        string sqlAthletics = String.Format(@"
                            SELECT
	                            TRIM(IR.id::varchar(10)) AS id
                            FROM
	                            involve_rec	IR	INNER JOIN	invl_table	IT	ON	TRIM(IR.invl)	=	TRIM(IT.invl)
												                            AND	IT.sanc_sport	=	'Y'
                            WHERE
	                            TODAY	BETWEEN	IR.beg_date AND NVL(IR.end_date, TODAY)
                            AND
	                            IT.invl	IN	({0})
                            GROUP BY
                                IR.id
                        ", athleticsList);

                        dtAthletics = cxConn.ConnectToERP(sqlAthletics, ref exAthletics);

                        if (exAthletics != null) { throw exAthletics; }
                        List<string> athleteIDs = dtAthletics.AsEnumerable().Select(athlete => athlete.Field<string>("id")).ToList();
                        var filteredRows = from row in dtResults.AsEnumerable()
                                           where athleteIDs.Contains(row.Field<string>("HostID"))
                                           select row;
                        dtResults = filteredRows == null || filteredRows.Count() == 0 ? new DataTable() : filteredRows.CopyToDataTable();
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while filtering facet search based on athletics", ex, null, true));
                    }
                    finally
                    {
                        if (cxConn.IsNotClosed()) { cxConn.Close(); }
                    }
                }

                #endregion

                #region Faceted Search - Residency

                List<ListItem> selectedResidency = cblResidency.Items.Cast<ListItem>().Where(li => li.Selected).ToList();
                if (selectedResidency.Count > 0)
                {
                    OdbcConnectionClass3 cxConn = helper.CONNECTION_CX_LIVE;
                    DataTable dtResidency = null;
                    Exception exResidency = null;

                    string residencyList = String.Format("'{0}'", String.Join("','", selectedResidency.Select(li => li.Value).ToList()));

                    string sqlResidency = String.Format(@"
                        SELECT
	                        TRIM(SSR.id::varchar(10)) AS id
                        FROM
	                        stu_serv_rec	SSR
                        WHERE
	                        SSR.yr	=	{0}
                        AND
	                        SSR.sess	=	'{1}'
                        AND
	                        SSR.intend_hsg	IN	({2})
                    ", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION, residencyList);

                    try
                    {

                        dtResidency = cxConn.ConnectToERP(sqlResidency, ref exResidency);
                        if (exResidency != null) { throw exResidency; }
                        List<string> residentIDs = dtResidency.AsEnumerable().Select(res => res.Field<string>("id")).ToList();
                        var filteredRows = from row in dtResults.AsEnumerable()
                                           where residentIDs.Contains(row.Field<string>("HostID"))
                                           select row;
                        dtResults = filteredRows == null || filteredRows.Count() == 0 ? new DataTable() : filteredRows.CopyToDataTable();
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException(String.Format("<p>Error while filtering facet search based on residency</p><p>{0}</p>", sqlResidency), ex, null, true));
                    }
                    finally
                    {
                        if (cxConn.IsNotClosed()) { cxConn.Close(); }
                    }
                }

                #endregion

                #region Faceted Search - Grad Candidacy

                if (!String.IsNullOrWhiteSpace(this.ddlGradCandidacy.SelectedValue))
                {
                    OdbcConnectionClass3 cxConn = helper.CONNECTION_CX_SP;
                    DataTable dtGrad = null;
                    Exception exGrad = null;

                    try
                    {
                        //string sqlGrad = String.Format("SELECT student_id FROM cc_stg_undergrad_candidacy WHERE datecreated >= TO_DATE('{0}', '%Y-%m-%d')", helper.START_DATE);
                        string sqlGrad = String.Format("EXECUTE PROCEDURE ci_admin_facetedsearch_undergradcandidacy('{0}')", helper.START_DATE);

                        dtGrad = cxConn.ConnectToERP(sqlGrad, ref exGrad);
                        if (exGrad != null) { throw exGrad; }
                        if (dtGrad != null && dtGrad.Rows.Count > 0)
                        {
                            //The "student_id" field coming back from CX is an int so it needs to be re-cast as a string for the LINQ comparisons below
                            List<string> gradIDs = dtGrad.AsEnumerable().Select(grad => grad.Field<int>("student_id").ToString()).ToList();
                            
                            //Generally "filteredRows" would be of type "var" but because it is initialized with null and used in one of the two branches, a data type must be specified
                            EnumerableRowCollection<DataRow> filteredRows = null;
                            if(this.ddlGradCandidacy.SelectedValue == "Y")
                            {
                                filteredRows = from row in dtResults.AsEnumerable()
                                               where gradIDs.Contains(row.Field<string>("HostID"))
                                               select row;
                            }
                            else
                            {
                                filteredRows = from row in dtResults.AsEnumerable()
                                               where !gradIDs.Contains(row.Field<string>("HostID"))
                                               select row;
                            }
                            dtResults = filteredRows == null || filteredRows.Count() == 0 ? new DataTable() : filteredRows.CopyToDataTable();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while filtering search on graduation candidacy.", ex, null, true));
                    }
                    finally
                    {
                        if (cxConn.IsNotClosed()) { cxConn.Close(); }
                    }
                }

                #endregion

                #region Include Additional Fields

                if (dtResults != null && dtResults.Rows.Count > 0)
                {
                    //Loop through all rows in the recordset. Processing occurs at this point because the recordset is in its smallest state since all filtering has been completed.
                    for (int ii = 0; ii < dtResults.Rows.Count; ii++)
                    {
                        //Establish database connection
                        OdbcConnectionClass3 cxSpConn = helper.CONNECTION_CX_SP;

                        //Initialize query variables
                        DataTable dtPER = null;
                        Exception exPER = null;

                        string sqlPER = String.Format(@"EXECUTE PROCEDURE ci_admin_facetedsearch_extrafields({0})", dtResults.Rows[ii]["HostID"].ToString());

                        try
                        {
                            dtPER = cxSpConn.ConnectToERP(sqlPER, ref exPER);
                            if (exPER != null) { throw exPER; }
                            if (dtPER != null && dtPER.Rows.Count > 0)
                            {
                                DataRow dr = dtPER.Rows[0];
                                dtResults.Rows[ii]["Admit Year"] = dr["adm_yr"].ToString();
                                dtResults.Rows[ii]["Admit Term"] = dr["adm_sess"].ToString();
                                dtResults.Rows[ii]["ClassCode"] = dr["cl"].ToString();
                                dtResults.Rows[ii]["AcademicStanding"] = dr["acad_stat"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            ciHelper.FormatException("Could not load program enrollment data in faceted search.", ex);
                        }
                        finally
                        {
                            //Always close database connection
                            if (cxSpConn.IsNotClosed()) { cxSpConn.Close(); }
                        }
                    }
                }

                #endregion
                ////////////////////////////////////////////////////////////////////
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while running the faceted search.", ex));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
            return dtResults;
        }
    }
}