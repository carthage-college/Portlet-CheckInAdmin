using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CUS.OdbcConnectionClass3;
using Portlet.CheckInStudent;
using Jenzabar.Common;
using Jenzabar.Portal.Framework;
//Export to Excel
using System.IO;
using System.Drawing;
using System.Data.SqlClient;
using System.Configuration;

using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Portlet.CheckInAdmin
{
    public class CheckInAdminHelper
    {
        Helper helper = new Helper();

        #region Screen Names

        public string VIEW_SEARCH
        {
            get { return "Search_Student"; }
        }

        public string VIEW_DETAIL
        {
            get { return "Detail_Student"; }
        }

        #endregion


        #region Viewstate Keys

        public string VIEWSTATE_SEARCH_STUDENTID
        {
            get { return "CheckInAdmin_StudentID"; }
        }

        public string VIEWSTATE_SEARCH_CRITERIA
        {
            get { return "CheckInAdmin_Search"; }
        }

        public string VIEWSTATE_SEARCH_RESULTS
        {
            get { return "CheckInAdmin_Results"; }
        }

        public string VIEWSTATE_MESSAGE
        {
            get { return "CheckInAdmin_Message"; }
        }

        #endregion

        public void CreateXLS(DataTable dt)
        {
            string strContentType = "text/plain",
                strFilename = "ErrorOutput.txt",
                fileName = "ExportedData";

            var mstream = new MemoryStream();
            var sw = new StreamWriter(mstream);
            var dgResults = CreateDataGrid();

            ConfigureDataGrid(ref dgResults,
                                dt,
                                true, //Show column headings
                                false, // Alternate row colors
                                true, // Show gridlines
                                5, //Cell padding
                                ""); //Column value


            dgResults.DataSource = dt;
            dgResults.DataBind();

            var stringWrite = new StringWriter();
            var htmlWrite = new HtmlTextWriter(stringWrite);
            dgResults.RenderControl(htmlWrite);

            htmlWrite.Flush();

            sw.WriteLine(stringWrite.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", ""));
            strContentType = "application/vnd.ms-excel";
            strFilename = fileName + ".xls";
        }

        public DataGrid CreateDataGrid()
        {
            var dgResults = new DataGrid
            {
                PageSize = 30,
                BorderWidth = 1,
                BorderStyle = BorderStyle.Solid//, BorderColor = System.Drawing.Color.FromArgb(224, 224, 224)
            };
            //dgResults.AlternatingItemStyle.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            dgResults.HeaderStyle.Font.Bold = true;
            return dgResults;
        }

        public void ConfigureDataGrid(ref DataGrid dgResults, DataTable dt, bool showColumnHeadings, bool useAlternatingRowColor, bool showGridLines, Int16 cellPadding, string columnLabels)
        {
            dgResults.ShowHeader = showColumnHeadings;

            //dgResults.AlternatingItemStyle.BackColor = useAlternatingRowColor ? dgResults.BorderColor : dgResults.BackColor;

            dgResults.GridLines = showGridLines ? GridLines.Both : GridLines.None;
            dgResults.BorderStyle = showGridLines ? BorderStyle.Solid : BorderStyle.None;

            dgResults.CellPadding = cellPadding;

            if (showColumnHeadings)
            {
                var strColumnLabel = columnLabels.Split(',');

                for (var ii = 0; ii < dt.Columns.Count; ii++)
                {
                    try
                    {
                        dt.Columns[ii].ColumnName = strColumnLabel[ii].Trim();
                    }
                    catch
                    {
                        //do nothing
                    }
                }
            }
        }

        public string GetTaskStatus(CheckInTasks task, string hostID)
        {
            return GetTaskStatus(task.ToDescriptionString(), hostID);
        }

        public string GetTaskStatus(string task, string hostID)
        {
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtTask = null;
            Exception exTask = null;
            string sql = String.Format(@"
                EXECUTE dbo.CUS_spCheckIn_TaskStatusForStudent @intStudentID = {0}, @strViewColumn = ?
            ", hostID);

            string taskStatus = "";

            List<OdbcParameter> paramTask = new List<OdbcParameter>()
            {
                new OdbcParameter("viewcolumn", task)
            };

            try
            {
                dtTask = spConn.ConnectToERP(sql, ref exTask, paramTask);
                if (exTask != null) { throw exTask; }
                if (dtTask != null && dtTask.Rows.Count > 0)
                {
                    taskStatus = dtTask.Rows[0]["TaskStatus"].ToString();
                }
            }
            catch (Exception ex)
            {
                helper.FormatException("An error occurred while looking for the status of a task", ex, true);
                return "Error";
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }

            return taskStatus;
        }

        public string FormatException(string label, Exception ex, PortalUser user = null, bool emailAdmin = false)
        {
            user = user == null ? PortalUser.Current : user;
            //return helper.FormatException(label, ex, emailAdmin, user);
            string errorMessage = String.Format("<p>{0}</p>", label);

            try
            {
                string hostID = String.IsNullOrWhiteSpace(user.HostID) ? "No CX ID" : int.Parse(user.HostID).ToString();

                //Administrators can see additional details about the exception
                errorMessage = String.Format("{0}<p>User: {1} {2} (ID: {3})</p><p>Message: {4}</p><p>Inner Exception Stack Trace: {5}</p><pre>Stack Trace: {6}</pre><br /><pre>Exception as string: {7}</pre>",
                    errorMessage,
                    user.FirstName,
                    user.LastName,
                    hostID,
                    (String.IsNullOrWhiteSpace(ex.Message) ? "No message" : ex.Message),
                    (ex.InnerException == null ? "No inner exception" : ex.InnerException.StackTrace),
                    //(String.IsNullOrWhiteSpace(ex.InnerException.ToString()) ? "" : ex.InnerException.ToString()),
                    (String.IsNullOrWhiteSpace(ex.StackTrace) ? "" : ex.StackTrace),
                    (ex.ToString())
                );
            }
            catch (Exception exInner)
            {

            }
            return errorMessage;
        }

        public string GetPortalIDByHostID(int cxID)
        {
            string portalID = "";
            DataTable dtPortal = null;
            Exception exPortal = null;
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            try
            {
                string sqlPortal = String.Format(@"SELECT ID FROM FWK_User WHERE HostID = {0}", cxID);
                dtPortal = jicsConn.ConnectToERP(sqlPortal, ref exPortal);
                if (exPortal != null) { throw exPortal; }
                if (dtPortal != null && dtPortal.Rows.Count > 0)
                {
                    portalID = dtPortal.Rows[0]["ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                FormatException("Error trying to match student ID with portal ID", ex, null, true);
            }
            return portalID;
        }

        public DataTable GetTasks()
        {
            string sqlTasks = String.Format(@"EXECUTE dbo.CUS_spCheckIn_Tasks");
            DataTable dtTasks = null;
            Exception exTasks = null;
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;

            try
            {
                dtTasks = spConn.ConnectToERP(sqlTasks, ref exTasks);
                if (exTasks != null) { throw exTasks; }
                if (dtTasks == null || dtTasks.Rows.Count == 0)
                {
                    throw new Exception("No tasks returned by the stored procedure.");
                }
            }
            catch (Exception ex)
            {
                FormatException("An exception occurred while retrieving task informamtion", ex, null, true);
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }
            return dtTasks;
        }

        public DataTable GetOfficeAndTask()
        {
            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtOffice = null;
            Exception exOffice = null;
//            string sqlOffice = @"
//	            SELECT
//		            O.OfficeName, O.OfficeID, OT.TaskName, OT.TaskID, OT.ViewColumn
//	            FROM
//		            CI_OfficeTaskSession	OTS	INNER JOIN	CI_OfficeTask	OT	ON	OTS.OfficeTaskID	=	OT.TaskID
//									            INNER JOIN	CI_Office		O	ON	OT.OfficeID			=	O.OfficeID
//	            WHERE
//		            OTS.ActiveYear		=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
//	            AND
//		            OTS.ActiveSession	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
//	            ORDER BY
//		            O.Sequence, OT.Sequence
//            ";
            string sqlOffice = "EXECUTE CUS_spCheckIn_GetOfficeAndTask";

            try
            {
                dtOffice = spConn.ConnectToERP(sqlOffice, ref exOffice);
                if (exOffice != null) { throw exOffice; }
            }
            catch (Exception ex)
            {
                FormatException("An exception occurred while loading office/task table", ex);
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }
            return dtOffice;
        }

        public List<string> GetTaskViewColumns()
        {
            DataTable dtTasks = GetTasks();
            return dtTasks.AsEnumerable().Select(task => task.Field<string>("ViewColumn")).ToList();
        }

        public string GenerateStudentMetaData()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            string debug = "";
            string sqlStudentsFromCX = "";
            if (helper.ACTIVE_SESSION == "RA")
            {
                //debug = String.Format("{0}<p>Load students for Fall</p>", debug);
                sqlStudentsFromCX = String.Format("EXECUTE PROCEDURE ci_get_students_fall({0})", helper.ACTIVE_YEAR);
            }
            else if (helper.ACTIVE_SESSION == "RC")
            {
                //debug = String.Format("{0}<p>Load students for Spring</p>", debug);
                sqlStudentsFromCX = String.Format("EXECUTE PROCEDURE ci_get_students_spring({0}, 'RA', {1}, 'RC')", helper.ACTIVE_YEAR - 1, helper.ACTIVE_YEAR);
            }
            else
            {
                debug = String.Format("{0}<p>Unknown term {1} for data load</p>", debug, helper.ACTIVE_SESSION);
            }

            OdbcConnectionClass3 cxSpConn = helper.CONNECTION_CX_SP;
            DataTable dtStudentsFromCX = null;
            List<int> listStudentsFromCX = new List<int>() { };
            Exception exStudentsFromCX = null;
            try
            {
                dtStudentsFromCX = cxSpConn.ConnectToERP(sqlStudentsFromCX, ref exStudentsFromCX);
                if (exStudentsFromCX != null) { throw exStudentsFromCX; }
                if (dtStudentsFromCX != null && dtStudentsFromCX.Rows.Count > 0)
                {
                    listStudentsFromCX = dtStudentsFromCX.AsEnumerable().Select(row => row.Field<int>("cx_id")).ToList();
                }
            }
            catch (Exception ex)
            {
                debug = String.Format("{0}<p>Encountered a problem getting students from CX:</p><p>{1}</p>", debug, this.FormatException("", ex));
            }

            sw.Stop();
            debug = String.Format("{0}<p>Time for getting {1} students from CX: {2}</p>", debug, listStudentsFromCX.Count, sw.Elapsed.ToString());
            sw.Reset();

            sw.Start();
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtStudentsFromJICS = null;
            Exception exStudentsFromJICS = null;
            string sqlStudentsFromJICS = String.Format("SELECT CAST(FU.HostID AS INT) AS HostID, SMD.IsActive FROM CI_StudentMetaData SMD INNER JOIN FWK_User FU ON SMD.UserID = FU.ID WHERE ActiveYear = {0} AND ActiveSession = '{1}'", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);
            try
            {
                dtStudentsFromJICS = jicsConn.ConnectToERP(sqlStudentsFromJICS, ref exStudentsFromJICS);
                if (exStudentsFromJICS != null) { throw exStudentsFromJICS; }
            }
            catch (Exception ex)
            {
                debug = String.Format("{0}<p>Encountered problem getting students from JICS:</p><p>{1}</p>", debug, this.FormatException("", ex));
            }

            //Initialize lists
            List<int> disableJICS = new List<int>() { }, enableJICS = new List<int>() { }, createJICS = new List<int>() { };

            //Identify which records need to be changed (created, enabled, or disabled)
            try
            {
                //Flip IsActive from 1 to 0
                disableJICS = dtStudentsFromJICS.AsEnumerable().Where(row => row.Field<bool>("IsActive") == true && !listStudentsFromCX.Contains(row.Field<int>("HostID"))).Select(row => row.Field<int>("HostID")).ToList();

                //Flip IsActive from 0 to 1
                enableJICS = dtStudentsFromJICS.AsEnumerable().Where(row => row.Field<bool>("IsActive") == false && listStudentsFromCX.Contains(row.Field<int>("HostID"))).Select(row => row.Field<int>("HostID")).ToList();

                //Create StudentMetaData record
                createJICS = listStudentsFromCX.Where(list => !dtStudentsFromJICS.AsEnumerable().Select(row => row.Field<int>("HostID")).Contains(list)).ToList();

                debug = String.Format("{0}<p>Enable {1} records<br />Disable {2} records<br />Create {3} records</p>", debug, enableJICS.Count, disableJICS.Count, createJICS.Count);
            }
            catch (Exception ex)
            {
                //debug = String.Format("{0}<p>Encountered problem while determining presence or absence of StudentMetaData:</p><p>{1}</p>", debug, ciHelper.FormatException("", ex));
            }
            sw.Stop();

            debug = String.Format("{0}<p>Time to assemble lists: {1}</p>", debug, sw.Elapsed.ToString());
            sw.Reset();

            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            Exception exSP = null;
            string sqlSP = "";

            #region Disable Student Meta Data

            sw.Start();
            foreach (int cxID in disableJICS)
            {
                try
                {
                    sqlSP = String.Format("EXECUTE CUS_spCheckIn_DisableStudentMetaData @intHostID = {0}", cxID);
                    jicsSpConn.ConnectToERP(sqlSP, ref exSP);
                    if (exSP != null) { throw exSP; }
                }
                catch (Exception ex)
                {
                    debug = String.Format("{0}<p>Error executing disable: {1}<br />{2}</p>", debug, sqlSP, this.FormatException("", ex));
                }
            }

            sw.Stop();
            debug = String.Format("{0}<p>Time to process disable list ({1}): {2}</p>", debug, disableJICS.Count, sw.Elapsed.ToString());
            sw.Reset();

            #endregion

            #region Enable/Create Student Meta Data

            sw.Start();
            List<int> combinedIDs = enableJICS.Union(createJICS).ToList();
            foreach (int cxID in combinedIDs)
            {
                try
                {
                    sqlSP = String.Format("EXECUTE CUS_spCheckIn_InsertUpdateStudentMetaData @intHostID = {0}", cxID);
                    jicsSpConn.ConnectToERP(sqlSP, ref exSP);
                    if (exSP != null) { throw exSP; }
                }
                catch (Exception ex)
                {
                    debug = String.Format("{0}<p>Error executing insert/update: {1}<br />{2}</p>", debug, sqlSP, this.FormatException("", ex));
                }
            }
            sw.Stop();
            debug = String.Format("{0}<p>Time to process create/update ({1}): {2}</p>", debug, combinedIDs.Count, sw.Elapsed.ToString());
            sw.Reset();

            #endregion

            #region Initialize Student Progress

            sw.Start();
            try
            {
                jicsSpConn.ConnectToERP("EXECUTE CUS_spCheckIn_InitializeStudentProgress", ref exSP);
                if (exSP != null) { throw exSP; }
            }
            catch (Exception ex)
            {
                debug = String.Format("{0}<p>Error when initializing student progress</p>", debug);
            }
            finally
            {
                sw.Stop();
                debug = String.Format("{0}<p>Time to process student progress initialization: {1}</p>", debug, sw.Elapsed.ToString());
                sw.Reset();
            }

            #endregion

            if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }

            return debug;
        }

        #region Faceted Search helpers

        public DataTable GetAthletics()
        {
            OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;
            DataTable dtAthletics = null;
            Exception exAthletics = null;

            string sqlAthletics = @"
                SELECT
                    TRIM(invl) AS involve_code, TRIM(txt) AS involve_text
                FROM
                    invl_table
                WHERE
                    sanc_sport = 'Y'
                AND
                    TODAY BETWEEN active_date AND NVL(inactive_date, TODAY)
                ORDER BY
                    txt";

            try
            {
                dtAthletics = cxConn.ConnectToERP(sqlAthletics, ref exAthletics);
                if (exAthletics != null) { throw exAthletics; }
            }
            catch (Exception ex)
            {
                FormatException("Could not retrieve athletic organizations", ex, null, true);
            }
            finally
            {
                if (cxConn.IsNotClosed()) { cxConn.Close(); }
            }
            return dtAthletics;
        }

        #endregion
    }

    public class Task
    {
        public string OfficeName
        {
            get { return this.OfficeName; }
            set { this.OfficeName = value; }
        }

        public string TaskName
        {
            get { return this.TaskName; }
            set { this.TaskName = value; }
        }

        public string Status_CX
        {
            get { return this.Status_CX; }
            set { this.Status_CX = value; }
        }

        public string Status_JICS
        {
            get { return this.Status_JICS; }
            set { this.Status_JICS = value; }
        }

        public DateTime? StatusDate
        {
            get { return this.StatusDate; }
            set { this.StatusDate = value; }
        }

        public string TaskID
        {
            get { return this.TaskID; }
            set { this.TaskID = value; }
        }

        public string Reason
        {
            get { return this.Reason; }
            set { this.Reason = value; }
        }

        public CarthageSystem? System
        {
            get { return this.System; }
            set { this.System = value; }
        }

        public string TaskStatus
        {
            //Derived value, cannot be explicitly set
            get
            {
                return this.Status_CX == "Y" || String.IsNullOrEmpty(this.Status_JICS) ? this.Status_CX : this.Status_JICS;
            }
        }

        public Task(string officeName, string taskName, string statusCX = null, string statusJICS = null, string taskID = null, DateTime? statusDate = null, string reason = null, CarthageSystem? system = null)
        {
            this.OfficeName = officeName;
            this.TaskName = taskName;
            this.Status_CX = statusCX;
            this.Status_JICS = statusJICS;
            this.TaskID = taskID;
            this.StatusDate = statusDate;
            this.Reason = reason;
            this.System = system;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}: {2}", this.OfficeName, this.TaskName, this.TaskStatus);
        }
    }
}
