﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
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
            string stopwatch = "";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            LoadStudentProgress();
            sw.Stop();

            stopwatch = String.Format("<p>Load student progress: {0}</p>", sw.Elapsed.ToString());

            sw.Reset();
            sw.Start();
            LoadStudentActivity();
            sw.Stop();

            stopwatch = String.Format("{0}<p>Load student activity: {1}</p>", stopwatch, sw.Elapsed.ToString());

            this.ParentPortlet.ShowFeedback(FeedbackType.Message, stopwatch);

            this.aRoot.Visible = PortalUser.Current.IsSiteAdmin;
        }

        #region Data Loading

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
            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtStudentProgressCounts = null;
            Exception exStudentProgressCounts = null;

            string sqlProgress = @"EXECUTE CUS_spCheckIn_GetStudentProgressSummary";
            
            try
            {
                //dtStudentProgressCounts = jicsConn.ConnectToERP(sqlProgress, ref exStudentProgressCounts);
                dtStudentProgressCounts = spConn.ConnectToERP(sqlProgress, ref exStudentProgressCounts);
                if (exStudentProgressCounts != null) { throw exStudentProgressCounts; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, helper.FormatException("An error occurred while retrieving student progress counts", ex));
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }

            #region Commented out
            //            OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;
//            DataTable dtUnfinishedCX = null;
//            Exception exUnfinishedCX = null;
//            string sqlUnfinishedCX = String.Format(@"
//                SELECT
//                    id
//                FROM
//                    stu_acad_rec
//                WHERE
//                    yr = {0}
//                AND
//                    sess = '{1}'
//                AND
//                    subprog IN ('TRAD','TRAP')
//                AND
//                    reg_stat <> 'C'
//            ", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);
//            try
//            {
//                dtUnfinishedCX = cxConn.ConnectToERP(sqlUnfinishedCX, ref exUnfinishedCX);
//                if (exUnfinishedCX != null) { throw exUnfinishedCX; }

//                jicsConn = helper.CONNECTION_JICS;

//                try
//                {
//                    DataTable dtNotStarted = null;
//                    Exception exNotStarted = null;
//                    string idsList = "0";
//                    if (dtUnfinishedCX.Rows.Count > 0)
//                    {
//                        idsList = string.Join(",", dtUnfinishedCX.AsEnumerable().Select(row => row.Field<int>("id")).ToArray());
//                    }
//                    string sqlNotStarted = String.Format(@"
//                        SELECT
//	                        COUNT(U.ID) AS NotStarted
//                        FROM
//	                        FWK_User	U	LEFT JOIN	CI_StudentProgress	SP	ON	U.ID	=	SP.UserID
//                        WHERE
//	                        CAST(HostID AS INT) IN ({0})
//                        AND
//	                        SP.ProgressID	IS	NULL
//                    ", idsList);

//                    dtNotStarted = jicsConn.ConnectToERP(sqlNotStarted, ref exNotStarted);
//                    if (exNotStarted != null) { throw exNotStarted; }
//                    if (dtNotStarted != null)
//                    {
//                        dtStudentProgressCounts.Rows[0]["NotStarted"] = dtNotStarted.Rows.Count.ToString();
//                    }
//                }
//                catch (Exception exUnstarted)
//                {
//                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error pulling students who have not started from portal", exUnstarted));
//                }
//            }
//            catch (Exception ex)
//            {
//                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while pulling list of students who have not started", ex));
//            }
//            finally
//            {
//                if (cxConn.IsNotClosed()) { cxConn.Close(); }
            //            }
            #endregion
            #endregion


            chartStudentProgress.DataSource = dtStudentProgressCounts;
            chartStudentProgress.DataBind();

            #region Commented out
            //            DataTable dtStudentStarted = ciHelper.GetMergedView();
//            DataTable dtStudentMissing1 = dtStudentStarted;

//            DataTable dtStudentComplete = ciHelper.GetCompletedStudents();

//            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;

//            DataTable dtStudentProgress = null;
//            Exception exStudentProgress = null;
//            string sqlStudentProgress = @"
//                SELECT
//                    SUM(CASE WHEN Summary.Completed = Summary.TotalTasks THEN 1 ELSE 0 END) AS 'Complete',
//                    SUM(CASE WHEN Summary.Completed = TotalTasks - 1 THEN 1 ELSE 0 END) AS 'Missing1',
//                    SUM(CASE WHEN Summary.Completed < TotalTasks - 1 THEN 1 ELSE 0 END) AS 'Started',
//                    0 AS 'NotStarted'
//                FROM
//                    (
//                        SELECT
//                            UserID, COUNT(*) AS 'Completed', (SELECT COUNT(*) FROM CI_OfficeTask) AS 'TotalTasks'
//                        FROM
//                            CI_StudentProgress
//                        WHERE
//                            TaskStatus IN ('Y','W')
//                        GROUP BY UserID
//                    )    Summary
//            ";

//            try
//            {
//                dtStudentProgress = jicsConn.ConnectToERP(sqlStudentProgress, ref exStudentProgress);
//                if (exStudentProgress != null) { throw exStudentProgress; }
//                dtStudentProgress.Rows[0]["NotStarted"] = StudentsNotStarted();
//                dtStudentProgress.Rows[0]["Complete"] = dtStudentComplete == null ? 0 : dtStudentComplete.Rows.Count;
//                chartStudentProgress.DataSource = dtStudentProgress;
//                chartStudentProgress.DataBind();
//            }
//            catch (Exception ex)
//            {
//                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while drawing student progress chart on dashboard", ex));
//            }
//            finally
//            {
//                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            //            }
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
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while drawing chart for student activity", ex));
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
        protected int StudentsNotStarted()
        {
            int studentsNotStarted = 0;

            #region Initialize CX variables

            //Initialize variables for CX query
            OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;
            DataTable dtCXStudents = null;
            Exception exCXStudents = null;

            string sqlCXStudents = String.Format(@"
                SELECT
                    id, firstname, lastname
                FROM
                    directory_vw
                WHERE
                    class_year IN ('FF','FR','FN','JR','PFF','PTR','SO','SR','UT')
                GROUP BY
                    id, firstname, lastname
            ");
            #endregion

            #region Initialize JICS variables
            //Initialize variables for JICS query
            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
            DataTable dtJICSStudents = null;
            Exception exJICSStudents = null;
//            string sqlJICSStudents = String.Format(@"
//                SELECT CAST(HostID AS INT) AS HostID FROM FWK_User U INNER JOIN CI_StudentProgress SP ON U.ID = SP.UserID WHERE TaskStatus <> 'N' AND HostID IS NOT NULL GROUP BY HostID
//            ");
            string sqlJICSStudents = "EXECUTE CUS_spCheckIn_AdminStudentsWithCompletedTask";
            #endregion

            try
            {
                //Get all undergraduate students including incoming freshmen
                dtCXStudents = cxConn.ConnectToERP(sqlCXStudents, ref exCXStudents);
                
                //If the attempt at retrieving all undergrad students failed, throw an error
                if (exCXStudents != null) { throw exCXStudents; }

                //As long as at least one student was found, continue
                if (dtCXStudents != null && dtCXStudents.Rows.Count > 0)
                {
                    try
                    {
                        //Get all students who have completed (or had waived) at least one check-in task
                        dtJICSStudents = spConn.ConnectToERP(sqlJICSStudents, ref exJICSStudents);
                        
                        //If the attempt at retrieving all students who have finished something in check-in fails, throw an error
                        if (exJICSStudents != null) { throw exJICSStudents; }

                        //As long as at least one student was returned from JICS, continue
                        if(dtJICSStudents != null && dtJICSStudents.Rows.Count > 0)
                        {
                            //Put the CX ids from the portal into a list
                            List<int> jicsIDs = dtJICSStudents.AsEnumerable().Select(row => row.Field<int>("HostID")).ToList();

                            //Count the number of students who appear in the CX list, but not in the JICS list
                            studentsNotStarted = dtCXStudents.AsEnumerable().Count(cx => !jicsIDs.Contains(cx.Field<int>("id")));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while retrieving students from JICS", ex));
                    }
                    finally
                    {
                        //Always close your database connections
                        if (spConn.IsNotClosed()) { spConn.Close(); }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while retrieving students from CX", ex));
            }
            finally
            {
                //Always close your database conenctions
                if (cxConn.IsNotClosed()) { cxConn.Close(); }
            }

            return studentsNotStarted;
        }

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
                        CheckInTaskStatus result = helper.updatePortalTaskStatusFromCX(dr["ViewColumn"].ToString(), dr["UserID"].ToString());
                        if (result == CheckInTaskStatus.Yes) { recordsCompleted++; }
                    }
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
            }
            catch (Exception ex)
            {
                ciHelper.FormatException("Error while getting list of all incomplete tasks", ex);
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
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
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while exporting incomplete tasks", ex, null, true));
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

        #endregion
    }
}
