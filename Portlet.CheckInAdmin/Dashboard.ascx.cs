﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.DataVisualization;
using System.Web.UI.DataVisualization.Charting;
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

using System.Diagnostics;


namespace Portlet.CheckInAdmin
{
    public partial class Dashboard : PortletViewBase
    {
        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            LoadStudentProgress();
            sw.Stop();

            //ciHelper.LogEvent(null, null, null, LogEventType.Info, String.Format("<p>Load student progress: {0}</p>", sw.Elapsed.ToString()), LogScreen.Dashboard);

            sw.Reset();
            sw.Start();
            LoadStudentActivity();
            sw.Stop();

            LoadCheckInSummary();

            //ciHelper.LogEvent(null, null, null, LogEventType.Info, String.Format("<p>Load student activity: {0}</p>", sw.Elapsed.ToString()), LogScreen.Dashboard);

            this.aRoot.Visible = PortalUser.Current.IsSiteAdmin;
        }

        #region Data Loading

        private void LoadCheckInSummary()
        {
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtSummary = null;
            Exception exSummary = null;

            string sqlSummary = "EXECUTE CUS_spCheckIn_AdminChartSummary";
            try
            {
                dtSummary = spConn.ConnectToERP(sqlSummary, ref exSummary);
                if (exSummary != null) { throw exSummary; }
                chartCheckInSummary.DataSource = dtSummary;
                chartCheckInSummary.DataBind();
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatExceptionMessage(ex));
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }
        }

        private void LoadStudentProgress()
        {
            #region Commented out
            //DataTable dtStudentProgress = ciHelper.StudentProgressCounts();
            //DataTable dtStudentProgressCounts = new DataTable();
            //dtStudentProgressCounts.Columns.AddRange(new DataColumn[]{
            //    new DataColumn("Complete"),
            //    new DataColumn("Missing1"),
            //    new DataColumn("Started"),
            //    new DataColumn("NotStarted")
            //});
            //DataRow dr = dtStudentProgressCounts.NewRow();

            //int totalTasks = ciHelper.GetTasks().Rows.Count;
            //dr["Complete"] = dtStudentProgress.AsEnumerable().Count(row => row.Field<int>("completed_task_count") + row.Field<int>("waived_task_count") == totalTasks);
            //dr["Missing1"] = dtStudentProgress.AsEnumerable().Count(row => row.Field<int>("completed_task_count") + row.Field<int>("waived_task_count") == totalTasks - 1);
            //dr["Started"] = dtStudentProgress.AsEnumerable().Count(row => row.Field<int>("completed_task_count") + row.Field<int>("waived_task_count") < totalTasks - 1 && row.Field<int>("completed_task_count") > 0);
            //dr["NotStarted"] = dtStudentProgress.AsEnumerable().Count(row => row.Field<int>("completed_task_count") == 0);
            //dtStudentProgressCounts.Rows.Add(dr);
            #endregion

            #region Faster Progress Count
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtStudentProgressCounts = null;
            Exception exStudentProgressCounts = null;

            string sqlProgress = @"EXECUTE CUS_spCheckIn_GetStudentProgressSummary";
            
            try
            {
                dtStudentProgressCounts = spConn.ConnectToERP(sqlProgress, ref exStudentProgressCounts);
                if (exStudentProgressCounts != null) { throw exStudentProgressCounts; }

                //foreach (DataRow dr in dtStudentProgressCounts.Rows)
                //{
                //    string seriesName = String.Format("Completed{0}", dr["TaskCount"].ToString());
                //    chartStudentProgress.Series.Add(seriesName);
                //    chartStudentProgress.Series[seriesName].ChartType = SeriesChartType.Column;
                //    chartStudentProgress.Series[seriesName].XValueMember = "TaskCount";
                //    chartStudentProgress.Series[seriesName].YValueMembers = "StudentCount";
                //    chartStudentProgress.Series[seriesName].IsValueShownAsLabel = true;
                //    //chartStudentProgress.Series[seriesName].IsVisibleInLegend = true;
                //}

                chartStudentProgress.ChartAreas["caStudentProgress"].AxisX.Maximum = dtStudentProgressCounts.Rows.Count - 1;
                chartStudentProgress.DataSource = dtStudentProgressCounts;
                chartStudentProgress.DataBind();
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, helper.FormatException("An error occurred while retrieving student progress counts", ex));
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }

            #endregion


            this.shStudentProgress.Text = String.Format("Student Progress for {0} {1}", helper.ACTIVE_SESSION_TEXT, helper.ACTIVE_YEAR);
        }

        private void LoadStudentActivity()
        {
            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;

            DataTable dtStudentActivity = null;
            Exception exStudentActivity = null;

            string sqlStudentActivity = "EXECUTE CUS_spCheckIn_AdminStudentActivity";

            #region Obsolete - 4 hour block logic
            /* 4-hour blocks */
            //            string sqlStudentActivity = String.Format(@"
            //                SELECT
            //                    HT.minTime, COUNT(SP.ProgressID) AS Completed,
            //                    CASE
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 0 AND 3        THEN    1
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 4 AND 7        THEN    2
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 8 AND 11    THEN    3
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 12 AND 15    THEN    4
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 16 AND 19    THEN    5
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 20 AND 23    THEN    6
            //                    END AS CmplHourSection,
            //                    CASE
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 0 AND 3        THEN    'Midnight - 3 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 4 AND 7        THEN    '4 a.m. - 7 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 8 AND 11    THEN    '8 a.m. - 11 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 12 AND 15    THEN    '12 p.m. - 3 p.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 16 AND 19    THEN    '4 p.m. - 7 p.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 20 AND 23    THEN    '8 p.m. - 11 p.m.'
            //                    END AS CmplHour
            //                FROM
            //                    (
            //                        --Borrowed liberally from https://stackoverflow.com/questions/11479918/include-missing-months-in-group-by-query
            //                        SELECT
            //                            DATEADD(HOUR, n * 4, DATEADD(HOUR, DATEDIFF(HOUR, 0, (SELECT CAST(MIN(CompletedOn) AS DATE) FROM CI_StudentProgress)), 0)) AS minTime
            //                        FROM (
            //                            SELECT TOP ((DATEDIFF(DAY, (SELECT CAST(MIN(CompletedOn) AS DATE) FROM CI_StudentProgress), (SELECT CAST(MAX(CompletedOn) AS DATE) FROM CI_StudentProgress)) + 1) * 6)
            //                                n = ROW_NUMBER() OVER (ORDER BY [object_id]) - 1
            //                            FROM sys.all_objects ORDER BY [object_id]
            //                        ) hourTable
            //                    ) AS HT    LEFT JOIN    CI_StudentProgress    SP    ON    HT.minTime                        <    SP.CompletedOn
            //                                                                AND    DATEADD(HOUR, 4, HT.minTime)    >    SP.CompletedOn
            //                WHERE
            //                    ISNULL(SP.TaskStatus,'Y') = 'Y'
            //                GROUP BY
            //                    HT.minTime,
            //                    CASE
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 0 AND 3        THEN    1
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 4 AND 7        THEN    2
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 8 AND 11    THEN    3
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 12 AND 15    THEN    4
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 16 AND 19    THEN    5
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 20 AND 23    THEN    6
            //                    END,
            //                    CASE
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 0 AND 3        THEN    'Midnight - 3 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 4 AND 7        THEN    '4 a.m. - 7 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 8 AND 11    THEN    '8 a.m. - 11 a.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 12 AND 15    THEN    '12 p.m. - 3 p.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 16 AND 19    THEN    '4 p.m. - 7 p.m.'
            //                        WHEN    DATEPART(HOUR, HT.minTime)    BETWEEN 20 AND 23    THEN    '8 p.m. - 11 p.m.'
            //                    END
            //                ORDER BY
            //                    HT.minTime
            //            ");
            #endregion

            try
            {
                //dtStudentActivity = jicsConn.ConnectToERP(sqlStudentActivity, ref exStudentActivity);
                dtStudentActivity = spConn.ConnectToERP(sqlStudentActivity, ref exStudentActivity);
                if (exStudentActivity != null) { throw exStudentActivity; }
                chartStudentActivity.DataSource = dtStudentActivity;
                chartStudentActivity.DataBind();
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while drawing chart for student activity", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("Error while drawing chart for student activity", ex, null, null, null, null, LogEventType.Error, LogScreen.Dashboard, sqlStudentActivity)
                );
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }

            this.shStudentActivity.Text = String.Format("Student Activity for {0} {1}", helper.ACTIVE_SESSION_TEXT, helper.ACTIVE_YEAR);
        }

        /// <summary>
        /// Calculate the number of traditional undergraduate students who have not yet completed or waived a check-in task
        /// </summary>
        /// <returns></returns>
//        protected int StudentsNotStarted()
//        {
//            int studentsNotStarted = 0;

//            #region Initialize CX variables

//            //Initialize variables for CX query
//            OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;
//            DataTable dtCXStudents = null;
//            Exception exCXStudents = null;

//            string sqlCXStudents = String.Format(@"
//                SELECT
//                    id, firstname, lastname
//                FROM
//                    directory_vw
//                WHERE
//                    class_year IN ('FF','FR','FN','JR','PFF','PTR','SO','SR','UT')
//                GROUP BY
//                    id, firstname, lastname
//            ");
//            #endregion

//            #region Initialize JICS variables
//            //Initialize variables for JICS query
//            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
//            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
//            DataTable dtJICSStudents = null;
//            Exception exJICSStudents = null;
////            string sqlJICSStudents = String.Format(@"
////                SELECT CAST(HostID AS INT) AS HostID FROM FWK_User U INNER JOIN CI_StudentProgress SP ON U.ID = SP.UserID WHERE TaskStatus <> 'N' AND HostID IS NOT NULL GROUP BY HostID
////            ");
//            string sqlJICSStudents = "EXECUTE CUS_spCheckIn_AdminStudentsWithCompletedTask";
//            #endregion

//            try
//            {
//                //Get all undergraduate students including incoming freshmen
//                dtCXStudents = cxConn.ConnectToERP(sqlCXStudents, ref exCXStudents);
                
//                //If the attempt at retrieving all undergrad students failed, throw an error
//                if (exCXStudents != null) { throw exCXStudents; }

//                //As long as at least one student was found, continue
//                if (dtCXStudents != null && dtCXStudents.Rows.Count > 0)
//                {
//                    try
//                    {
//                        //Get all students who have completed (or had waived) at least one check-in task
//                        dtJICSStudents = spConn.ConnectToERP(sqlJICSStudents, ref exJICSStudents);
                        
//                        //If the attempt at retrieving all students who have finished something in check-in fails, throw an error
//                        if (exJICSStudents != null) { throw exJICSStudents; }

//                        //As long as at least one student was returned from JICS, continue
//                        if(dtJICSStudents != null && dtJICSStudents.Rows.Count > 0)
//                        {
//                            //Put the CX ids from the portal into a list
//                            List<int> jicsIDs = dtJICSStudents.AsEnumerable().Select(row => row.Field<int>("HostID")).ToList();

//                            //Count the number of students who appear in the CX list, but not in the JICS list
//                            studentsNotStarted = dtCXStudents.AsEnumerable().Count(cx => !jicsIDs.Contains(cx.Field<int>("id")));
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while retrieving students from JICS", ex));
//                    }
//                    finally
//                    {
//                        //Always close your database connections
//                        if (spConn.IsNotClosed()) { spConn.Close(); }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while retrieving students from CX", ex));
//            }
//            finally
//            {
//                //Always close your database conenctions
//                if (cxConn.IsNotClosed()) { cxConn.Close(); }
//            }

//            return studentsNotStarted;
//        }

        #endregion

        #region Event Handlers
        protected void aFacetSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.ChangeScreen("Facet_Search");
        }

        protected void aNameSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.ChangeScreen("Search_Student");
        }

        protected void btnUpdateSMD_Click(object sender, EventArgs e)
        {
            string feedback = ciHelper.GenerateStudentMetaData();
            ciHelper.LogEvent(null, null, null, LogEventType.Info, feedback, LogScreen.Dashboard);
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, feedback);
        }

        protected void btnUpdateRegStat_Click(object sender, EventArgs e)
        {
            string feedback = "<p>Begin processing reg_stat information</p>";
            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            DataTable dtIncomplete = null;
            Exception exIncomplete = null;
            int recordsCompleted = 0;
            try
            {
                //Get every incomplete task for every active student
                string sqlIncomplete = String.Format("EXECUTE CUS_spCheckIn_GetIncompleteTasks");
                dtIncomplete = jicsSpConn.ConnectToERP(sqlIncomplete, ref exIncomplete);
                if (exIncomplete != null) { throw exIncomplete; }

                if (dtIncomplete != null && dtIncomplete.Rows.Count > 0)
                {
                    //For each task, execute the appropriate CX stored procedure to determine if it has been completed outside the check-in process
                    foreach (DataRow dr in dtIncomplete.Rows)
                    {
                        try
                        {
                            CheckInTaskStatus result = helper.updatePortalTaskStatusFromCX(dr["ViewColumn"].ToString(), dr["UserID"].ToString(), helper.ACTIVE_YEAR, helper.ACTIVE_SESSION, int.Parse(dr["HostID"].ToString()));
                            if (result == CheckInTaskStatus.Yes) { recordsCompleted++; }
                        }
                        catch (Exception ex)
                        {
                            //feedback = String.Format("{0}<p>Error while updating {1} for {2}<br />Message: {3}</p>", feedback, dr["ViewColumn"].ToString(), dr["UserID"].ToString(), ciHelper.FormatException("", ex));
                            feedback = String.Format("{0}<p>Error while updating {1} for {2}<br />Message: {3}</p>", feedback, dr["ViewColumn"].ToString(), dr["UserID"].ToString(),
                                    ciHelper.FormatException("", ex, null, null, null, LogEventType.Error, LogScreen.Dashboard, sqlIncomplete)
                            );
                        }
                    }
                    //DataRow dr = dtIncomplete.Rows[0];
                    //try
                    //{
                    //    CheckInTaskStatus result = helper.updatePortalTaskStatusFromCX(dr["ViewColumn"].ToString(), dr["UserID"].ToString(), helper.ACTIVE_YEAR, helper.ACTIVE_SESSION, int.Parse(dr["HostID"].ToString()));
                    //    if (result == CheckInTaskStatus.Yes) { recordsCompleted++; }
                    //}
                    //catch (Exception ex)
                    //{
                    //    feedback = String.Format("{0}<p>Error while updating {1} for {2}<br />Message: {3}</p>", feedback, dr["ViewColumn"].ToString(), dr["UserID"].ToString(), ciHelper.FormatException("", ex));
                    //}

                    feedback = String.Format("{0}<p>{1} incomplete tasks found; {2} were resolved.</p>", feedback, dtIncomplete.Rows.Count, recordsCompleted);

                    //Once the task statuses are current, run the process to update the "CompletedOn" field in CI_StudentMetaData and return the affected records so the CX updates can be made
                    DataTable dtComplete = null;
                    Exception exComplete = null;
                    string sqlComplete = "EXECUTE CUS_spCheckIn_AdminProcessCompleted";
                    try
                    {
                        dtComplete = jicsSpConn.ConnectToERP(sqlComplete, ref exComplete);
                        if (exComplete != null) { throw exComplete; }
                        if (dtComplete != null && dtComplete.Rows.Count > 0)
                        {
                            feedback = String.Format("{0}<p>Preparing to update reg_stat for {1} record(s)</p>", feedback, dtComplete.Rows.Count);
                            OdbcConnectionClass3 cxSpConn = helper.CONNECTION_CX_SP;
                            int updateCount = 0;
                            string debugFailedID = "";
                            foreach (DataRow drComplete in dtComplete.Rows)
                            {
                                Exception exRegStat = null;
                                string sqlRegStat = String.Format("EXECUTE PROCEDURE ci_registrar_set_regstat({0}, {1}, '{2}')", drComplete["HostID"].ToString(), helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);
                                try
                                {
                                    cxSpConn.ConnectToERP(sqlRegStat, ref exRegStat);
                                    if (exRegStat != null) { throw exRegStat; }
                                    updateCount++;
                                }
                                catch (Exception ex)
                                {
                                    debugFailedID = String.Format("{0}<p>Failed to execute: {1}</p>", debugFailedID, sqlRegStat);
                                }
                            }
                            feedback = String.Format("{0}<p>Completed reg_stat update for {1} record(s)</p>", feedback, updateCount);
                            if (cxSpConn.IsNotClosed()) { cxSpConn.Close(); }
                            
                            //Send list of errors to administrator
                            if (!String.IsNullOrWhiteSpace(debugFailedID)) { ciHelper.FormatException(debugFailedID, new Exception(), null, true); }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error processing completed students", ex, null, true));
                    }
                }
                else
                {
                    feedback = String.Format("{0}<p>No incomplete records found</p>", feedback);
                }
            }
            catch (Exception ex)
            {
                //feedback = String.Format("{0}<p>{1}</p>", feedback, ciHelper.FormatException("Error while getting list of all incomplete tasks", ex));
                feedback = String.Format("{0}<p>{1}</p>", feedback, ciHelper.FormatException("Error while getting list of all incomplete tasks", ex, null, null, null, LogEventType.Error, LogScreen.Dashboard));
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
            ciHelper.LogEvent(null, null, null, LogEventType.Info, feedback, LogScreen.Dashboard);
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, feedback);
        }

        protected void aRoot_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.ChangeScreen("SiteAdminTools");
        }

        protected void btnIncomplete_Click(object sender, EventArgs e)
        {
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtIncomplete = null;
            Exception exIncomplete = null;
            string sqlIncomplete = "EXECUTE dbo.CUS_spCheckIn_ExportIncompleteTasks";

            try
            {
                dtIncomplete = spConn.ConnectToERP(sqlIncomplete, ref exIncomplete);
                if (exIncomplete != null) { throw exIncomplete; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while exporting incomplete tasks", ex, null, null, null, LogEventType.Error, LogScreen.Dashboard, sqlIncomplete));
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }

            this.gvIncomplete.DataSource = dtIncomplete;
            this.gvIncomplete.DataBind();
            this.gvIncomplete.Visible = true;

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Check-In Students with Incomplete Tasks.xls");
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
                gvIncomplete.RenderBeginTag(hw);
                gvIncomplete.HeaderRow.RenderControl(hw);
                foreach (GridViewRow row in gvIncomplete.Rows)
                {
                    row.RenderControl(hw);
                }
                gvIncomplete.FooterRow.RenderControl(hw);
                gvIncomplete.RenderEndTag(hw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }

            this.gvIncomplete.Visible = false;
        }

        protected void btnNotStarted_Click(object sender, EventArgs e)
        {

        }

        #endregion

        public void ExportFile(DataTable dtQueryResults, string fileName = "Check-In Export File")
        {
            var mstream = new MemoryStream();
            var sw = new StreamWriter(mstream);
            var dgResults = ciHelper.CreateDataGrid();

            ciHelper.ConfigureDataGrid(ref dgResults, dtQueryResults, true, false, true, 5, "");

            dgResults.DataSource = dtQueryResults;
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
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}.xls", fileName));
            Response.AddHeader("Content-Length", byteArray.Length.ToString(CultureInfo.InvariantCulture));
            Response.ContentType = "application/octet-stream";
            Response.BinaryWrite(byteArray);
            Response.End();
        }
    }
}
