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


namespace Portlet.CheckInAdmin
{
    public partial class Dashboard : PortletViewBase
    {
        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadStudentProgress();

            LoadStudentActivity();
        }

        #region Data Loading

        /// <summary>
        /// Load the DataTable with the list of DataRows
        /// </summary>
        /// <param name="dt">The DataTable with the initial batch of search results</param>
        /// <param name="drList">Collection of DataRow objects which make up the new dataset</param>
        private void UpdateDataTable(ref DataTable dt, List<DataRow> drList)
        {
            if (drList.Count == 0) { dt.Rows.Clear(); }
            else { dt = drList.CopyToDataTable(); }
        }

        private void LoadStudentProgress()
        {
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

            #region Faster Progress Count
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtStudentProgressCounts = null;
            Exception exStudentProgressCounts = null;
//            string sqlProgress = @"
//                SELECT
//	                SUM(CASE Summary.IsComplete WHEN 'Y' THEN 1 ELSE 0 END) AS Complete,
//	                SUM(CASE Summary.IsMissing1 WHEN 'Y' THEN 1 ELSE 0 END) AS Missing1,
//	                SUM(CASE Summary.[Started] WHEN 'Y' THEN 1 ELSE 0 END) AS [Started],
//                    -1 AS NotStarted
//                FROM
//	                (
//		                SELECT
//			                UserID, COUNT(*) AS Complete,
//			                CASE WHEN COUNT(*) = (SELECT COUNT(*) FROM CI_OfficeTask) THEN 'Y' ELSE 'N' END AS IsComplete,
//			                CASE WHEN COUNT(*) = (SELECT COUNT(*) FROM CI_OfficeTask) - 1 THEN 'Y' ELSE 'N' END AS IsMissing1,
//			                CASE WHEN COUNT(*) < (SELECT COUNT(*) FROM CI_OfficeTask) - 1 THEN 'Y' ELSE 'N' END AS 'Started'
//		                FROM
//			                CI_StudentProgress	SP
//		                WHERE
//			                SP.TaskStatus	IN	('Y','W')
//		                AND
//			                SP.Yr	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
//		                AND
//			                SP.Sess	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
//		                GROUP BY
//			                UserID
//	                )	Summary
//            ";

            string sqlProgress = @"
	            SELECT
		            SUM(CASE WHEN Summary.IncompleteTaskCount = 0 THEN 1 ELSE 0 END) AS 'Complete',
		            SUM(CASE WHEN Summary.IncompleteTaskCount = 1 THEN 1 ELSE 0 END) AS 'Missing1',
		            SUM(CASE WHEN Summary.CompleteTaskCount > 0 AND Summary.IncompleteTaskCount > 1 THEN 1 ELSE 0 END) AS 'Started',
		            SUM(CASE WHEN Summary.CompleteTaskCount = 0 THEN 1 ELSE 0 END) AS 'NotStarted'
	            FROM
		            (
			            SELECT
				            U.ID, CAST(U.HostID AS INT) AS HostID, U.LastName, U.FirstName, SMD.IsActive, SMD.IsCheckedIn, SMD.FirstAccess, SMD.LastAccess,
				            SUM(CASE WHEN SP.TaskStatus IN ('Y','W') THEN 1 ELSE 0 END) AS CompleteTaskCount,
				            SUM(CASE WHEN SP.TaskStatus IN ('N','P') THEN 1 ELSE 0 END) AS IncompleteTaskCount
			            FROM
				            CI_StudentMetaData	SMD	INNER JOIN	FWK_User			U	ON	SMD.UserID			=	U.ID
										            LEFT JOIN	CI_StudentProgress	SP	ON	SMD.UserID			=	SP.UserID
																			            AND	SMD.ActiveYear		=	SP.Yr
																			            AND	SMD.ActiveSession	=	SP.Sess
			            WHERE
				            SMD.ActiveYear		=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
			            AND
				            SMD.ActiveSession	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
			            GROUP BY
				            U.ID, HostID, U.LastName, U.FirstName, SMD.IsActive, SMD.IsCheckedIn, SMD.FirstAccess, SMD.LastAccess
			            --ORDER BY
			            --	U.LastName, U.FirstName
		            )	Summary";
            
            try
            {
                dtStudentProgressCounts = jicsConn.ConnectToERP(sqlProgress, ref exStudentProgressCounts);
                if (exStudentProgressCounts != null) { throw exStudentProgressCounts; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, helper.FormatException("An error occurred while retrieving student progress counts", ex));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }

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


            chartStudentProgress.DataSource = dtStudentProgressCounts;
            chartStudentProgress.DataBind();

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

            this.shStudentProgress.Text = String.Format("Student Progress for {0} {1}", helper.ACTIVE_SESSION_TEXT, helper.ACTIVE_YEAR);
        }

        private void LoadStudentActivity()
        {
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;

            DataTable dtStudentActivity = null;
            Exception exStudentActivity = null;
//            string sqlStudentActivity = String.Format(@"
//                SELECT
//                    SUBSTRING(CONVERT(VARCHAR(10), HT.minTime, 7),1, 6) AS DateLabel, DATEPART(DAYOFYEAR, HT.minTime) AS Sequence, COUNT(SP.ProgressID) AS Completed
//                FROM
//                    (
//                        SELECT
//                            DATEADD(DAY, n, DATEADD(DAY, DATEDIFF(DAY, 0, (SELECT CAST(MIN(CompletedOn) AS DATE) FROM CI_StudentProgress)), 0)) AS minTime
//                        FROM (
//                            SELECT TOP ((DATEDIFF(DAY, (SELECT CAST(MIN(CompletedOn) AS DATE) FROM CI_StudentProgress), (SELECT CAST(MAX(CompletedOn) AS DATE) FROM CI_StudentProgress)) + 1))
//                                n = ROW_NUMBER() OVER (ORDER BY [object_id]) - 1
//                            FROM sys.all_objects ORDER BY [object_id]
//                        ) dayTable
//                    ) AS HT    LEFT JOIN    CI_StudentProgress    SP    ON    HT.minTime                        <    SP.CompletedOn
//                                                                AND    DATEADD(DAY, 1, HT.minTime)        >    SP.CompletedOn
//                WHERE
//                    ISNULL(SP.TaskStatus,'Y') = 'Y'
//                GROUP BY
//                    SUBSTRING(CONVERT(VARCHAR(10), HT.minTime, 7),1, 6), DATEPART(DAYOFYEAR, HT.minTime)
//                ORDER BY
//                    Sequence
//            ");
            string sqlStudentActivity = String.Format(@"
	            SELECT
		            CONVERT(CHAR(8), SP.CompletedOn, 112) AS DateOrder, SUBSTRING(CONVERT(VARCHAR(8), SP.CompletedOn, 1), 1, 5) AS DateLabel, COUNT(*) AS Completed
	            FROM
		            CI_StudentProgress	SP
	            WHERE
		            SP.Yr	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
	            AND
		            SP.Sess	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
	            AND
		            SP.TaskStatus	=	'Y'
	            GROUP BY
		            CONVERT(CHAR(8), SP.CompletedOn, 112), SUBSTRING(CONVERT(VARCHAR(8), SP.CompletedOn, 1), 1, 5)
	            ORDER BY
		            DateOrder
            ");

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

            try
            {
                dtStudentActivity = jicsConn.ConnectToERP(sqlStudentActivity, ref exStudentActivity);
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
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
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
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtJICSStudents = null;
            Exception exJICSStudents = null;
            string sqlJICSStudents = String.Format(@"
                SELECT CAST(HostID AS INT) AS HostID FROM FWK_User U INNER JOIN CI_StudentProgress SP ON U.ID = SP.UserID WHERE TaskStatus <> 'N' AND HostID IS NOT NULL GROUP BY HostID
            ");
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
                        dtJICSStudents = jicsConn.ConnectToERP(sqlJICSStudents, ref exJICSStudents);
                        
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
                        if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
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

        #endregion

        protected void btnIncomplete_Click(object sender, EventArgs e)
        {
            #region New Take on Incompletes
            DataTable dtIncomplete = null;

            OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            DataTable dtPortalComplete = null;
            Exception exPortalComplete = null;
            string sqlPortalComplete = @"
		        SELECT
			        CAST(U.HostID AS INT) AS HostID
		        FROM
			        CI_StudentProgress	SP	INNER JOIN	FWK_User	U	ON	SP.UserID	=	U.ID
		        WHERE
			        SP.TaskStatus	IN	('Y','W')
		        AND
			        SP.Yr	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
		        AND
			        SP.Sess	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
		        GROUP BY
			        HostID
				HAVING
					COUNT(*) = (SELECT COUNT(*) FROM CI_OfficeTask)
            ";
            try
            {
                dtPortalComplete = jicsConn.ConnectToERP(sqlPortalComplete, ref exPortalComplete);
                if (exPortalComplete != null) { throw exPortalComplete; }

                Exception exCxIncomplete = null;
                OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;

                //Initialize to "0" for early in the process when no students have completed check-in
                string idsAsCommaList = "0";
                if (dtPortalComplete.Rows.Count > 0) {
                    idsAsCommaList = string.Join(",", dtPortalComplete.AsEnumerable().Select(row => row.Field<int>("id")).ToArray());
                }
                string sqlCxIncomplete = String.Format(@"
                    SELECT
	                    DIR.id, DIR.lastname, DIR.firstname, DIR.email, DIR.phone
                    FROM
	                    stu_acad_rec	SAR	INNER JOIN	directory_vw	DIR	ON	SAR.id	=	DIR.id
                    WHERE
	                    SAR.yr = {0}
                    AND
	                    SAR.sess = '{1}'
                    AND
	                    SAR.reg_stat <> 'C'
                    AND
	                    SAR.subprog IN  ('TRAD','TRAP')
                    AND
	                    SAR.id NOT IN ({2})
                ", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION, idsAsCommaList);

                try
                {
                    dtIncomplete = cxConn.ConnectToERP(sqlCxIncomplete, ref exCxIncomplete);
                    if (exCxIncomplete != null) { throw exCxIncomplete; }
                }
                catch (Exception exCX)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error retrieving incomplete useres from JICS", exCX, null, true));
                }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error retrieving incomplete users from JICS", ex, null, true));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
            #endregion

            //DataTable dtIncomplete = ciHelper.GetIncompleteStudents();
            this.gvIncomplete.DataSource = dtIncomplete;
            this.gvIncomplete.DataBind();

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
        }

        protected void btnUpdateProgress_Click(object sender, EventArgs e)
        {
            DataTable dtCX = ciHelper.GetCXView();
            DataTable dtJICS = ciHelper.GetStudentProgress();

            string debug = "";

            List<string> taskList = ciHelper.GetTaskViewColumns();
            foreach (string task in taskList)
            {
                List<int> jicsStudentsCompletedTask = dtJICS.AsEnumerable().Where(stu => stu.Field<string>("ViewColumn") == task && stu.Field<string>("TaskStatus") == CheckInTaskStatus.Yes.ToDescriptionString()).Select(stu => stu.Field<int>("HostID")).ToList();
                List<int> cxStudentsCompletedTask = dtCX.AsEnumerable().Where(stu => stu.Field<string>(task) == CheckInTaskStatus.Yes.ToDescriptionString() && !jicsStudentsCompletedTask.Contains(stu.Field<int>("id"))).Select(stu => stu.Field<int>("id")).ToList();

                //foreach (int cxID in cxStudentsCompletedTask)
                //{
                //    helper.completeTask(task, cxID.ToString(), CarthageSystem.CX);
                //}

                debug = String.Format("{0}<p>Complete {1} task(s) for {2}<br />", debug, cxStudentsCompletedTask.Count.ToString(), task);

                List<int> jicsStudentsWaiveTask = dtJICS.AsEnumerable().Where(stu => stu.Field<string>("ViewColumn") == task && stu.Field<string>("TaskStatus") == CheckInTaskStatus.Waived.ToDescriptionString()).Select(stu => stu.Field<int>("HostID")).ToList();
                List<int> cxStudentsWaiveTask = dtCX.AsEnumerable().Where(stu => stu.Field<string>(task) == CheckInTaskStatus.Waived.ToDescriptionString() && !jicsStudentsWaiveTask.Contains(stu.Field<int>("id"))).Select(stu => stu.Field<int>("id")).ToList();

                debug = String.Format("{0}Waive {1} task(s) for {2}</p>", debug, cxStudentsWaiveTask.Count.ToString(), task);
            }
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, debug);
        }
    }
}
