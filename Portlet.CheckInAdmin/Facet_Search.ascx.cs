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

        /// <summary>
        /// Load the DataTable with the list of DataRows
        /// </summary>
        /// <param name="dt">The DataTable with the initial batch of search results</param>
        /// <param name="drList">Collection of DataRow objects which make up the new dataset</param>
        [Obsolete]
        private void UpdateDataTable(ref DataTable dt, List<DataRow> drList)
        {
            if (drList.Count == 0) { dt.Rows.Clear(); }
            else { dt = drList.CopyToDataTable(); }
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
            //string taskNoSpace = taskName.Replace(" ", "");
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

        [Obsolete]
        protected void btnExportExcel_old_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=CheckInExport.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);

                /****************************************************************************
                 * Ordinarily, this control would be written using the RenderControl() method
                 * but because all JICS portlets exist as .ascx files, we do not have access
                 * to the <form> tag which causes an exception to be thrown. The simplest
                 * solution to circumvent the exception is outlined in this article:
                 * http://stackoverflow.com/questions/6343630/gridview-must-be-placed-inside-a-form-tag-with-runat-server-even-after-the-gri
                 * 
                 * Essentially, the control is explicitly rendered step-by-step. Another option
                 * is to remove the GridView from the page's controls collection while performing
                 * the rendering and then re-adding it before the page loads. This approach was
                 * deemed more straightforward.
                *****************************************************************************/
                dgResults.RenderBeginTag(hw);
                dgResults.HeaderRow.RenderControl(hw);
                foreach (GridViewRow row in dgResults.Rows)
                {
                    row.RenderControl(hw);
                }
                dgResults.FooterRow.RenderControl(hw);
                dgResults.RenderEndTag(hw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }

        protected void aNameSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.NextScreen("Search_Student");
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            DataTable dtResults = GetSearchResults();
            this.dgResults.DataSource = dtResults;
            this.dgResults.DataBind();

            //Update the recordcount
            this.panelResultCount.Visible = this.btnExportExcel.Visible = true;
            this.ltlResultCount.Text = dtResults.Rows.Count.ToString();
        }

        #endregion

        protected DataTable GetSearchResults()
        {
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtResults = null;
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
                    CAST(CAST(U.HostID AS INT) AS VARCHAR(10)) AS HostID, U.LastName AS 'Last Name', U.FirstName AS 'First Name', U.Email, '' AS Standing, '' AS Athlete, '' AS Residency {0}
                FROM
                    CI_StudentMetaData  SMD INNER JOIN  FWK_User    U   ON  SMD.UserID  =   U.ID
                                            {1}
                WHERE
                    SMD.ActiveYear      =   (SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
                AND
                    SMD.ActiveSession   =   (SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
                AND
                    SMD.IsActive        =   1
                {2}
                ORDER BY
                    U.LastName, U.FirstName, U.Email
            ", sqlSelect, sqlFrom, sqlWhere);

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

        [Obsolete]
        protected void btnSearch_old_Click(object sender, EventArgs e)
        {
            DataTable dtCX = ciHelper.GetCXView();
            DataTable dtJICS = ciHelper.GetStudentProgress();

            DataTable dtResults = ciHelper.GetMergedView();

            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            string sqlSearch = "";

            try
            {
                //List<string> standing = cblStanding.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => li.Value).ToList();
                //if (standing.Count > 0)
                //{
                //    List<DataRow> results = dtResults.AsEnumerable().Where(row => standing.Contains(row.Field<string>("isfreshmantransfer"))).ToList();
                //    UpdateDataTable(ref dtResults, results);
                //}

                //List<string> athlete = cblAthlete.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => li.Value).ToList();
                //if (athlete.Count > 0)
                //{
                //    List<DataRow> results = dtResults.AsEnumerable().Where(row => athlete.Contains(row.Field<string>("is_athlete"))).ToList();
                //    UpdateDataTable(ref dtResults, results);
                //}

                List<string> residency = cblResidency.Items.Cast<ListItem>().Where(li => li.Selected).Select(li => li.Value).ToList();
                if (residency.Count > 0)
                {
                    List<DataRow> results = dtResults.AsEnumerable().Where(row => residency.Contains(row.Field<string>("resident_commuter"))).ToList();
                    UpdateDataTable(ref dtResults, results);
                }

                sqlSearch = String.Format("Current result count = {0}; cx {1}; jics {2}<br /><br />", dtResults == null ? "null" : dtResults.Rows.Count.ToString(), dtCX == null ? "null" : dtCX.Rows.Count.ToString(), dtJICS == null ? "null" : dtJICS.Rows.Count.ToString());

                try
                {
                    DataTable dtTasks = ciHelper.GetTasks();
                    List<string> viewColumns = dtTasks.AsEnumerable().Select(task => task.Field<string>("ViewColumn")).ToList();

                    foreach (string viewColumn in viewColumns)
                    {
                        List<RadioButton> radios = GetRadioGroup(this.tblOffices, String.Format("Task{0}", viewColumn));
                        //sqlSearch = String.Format("{0}<br />Found {1} radio buttons for {2}", sqlSearch, radios.Count, viewColumn);
                        RadioButton selectedRadio = radios.AsEnumerable().FirstOrDefault(rb => rb.Checked == true && !rb.ID.EndsWith("*")) ?? new RadioButton();

                        if (radios.Contains(selectedRadio))
                        {
                            //sqlSearch = String.Format("{0}<br />Value of matched radio for {1} is {2}", sqlSearch, viewColumn, selectedRadio.ID.Substring(selectedRadio.ID.Length - 1, 1));

                            //The task status: Y, N, W, or P
                            //string status = selectedRadio.ID.Substring(selectedRadio.ID.Length - 1, 1);
                            string[] status = selectedRadio.ID.Split('_');
                            List<string> listStatus = status.Last().Select(chr => chr.ToString()).ToList();
                            
                            

                            //The column name in the view (see CI_OfficeTask.ViewColumn)
                            //string col = selectedRadio.ID.Substring(0, selectedRadio.ID.Length - 1).Replace("radioTask", "");
                            string col = selectedRadio.ID.Replace("radioTask", "").Replace(String.Format("_{0}", status.Last()), "");

                            //Filter the results of the query based on the task and status of the checked element
                            //List<DataRow> results = dtResults.AsEnumerable().Where(row => row.Field<string>(col) == status).ToList();
                            List<DataRow> results = dtResults.AsEnumerable().Where(row => listStatus.Contains(row.Field<string>(col))).ToList();

                            //Update the datatable with the filtered results of the facet
                            UpdateDataTable(ref dtResults, results);

                            //If the filtering results in no rows remaining, break out of the loop to avoid continued searching against an empty recordset
                            if (dtResults == null || dtResults.Rows.Count == 0) { break; }
                        }
                    }

                    //Load the results into the GridView
                    this.dgResults.DataSource = dtResults;
                    this.dgResults.DataBind();

                    //Update the recordcount
                    this.panelResultCount.Visible = this.btnExportExcel.Visible = true;
                    this.ltlResultCount.Text = dtResults.Rows.Count.ToString();

                    //this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("SELECT ID, CAST(HostID AS INT) AS CXID, FirstName, LastName FROM FWK_User WHERE 1 = 1 {0} ORDER BY LastName, FirstName", sqlSearch));
                    //this.ParentPortlet.ShowFeedback(FeedbackType.Message, sqlSearch);
                }
                catch (Exception ex)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An error occurred while building the search string", ex));
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An error occurred while processing search criteria", ex, null, true));
            }
        }
    }
}