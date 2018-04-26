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

        [Obsolete]
        public DataTable GetCXView()
        {
            OdbcConnectionClass3 cxConn = helper.CONNECTION_CX;
            DataTable dtView = null;
            Exception exView = null;
            string sqlView = String.Format(@"
                SELECT
                    --General Info
                    DIR.id, DIR.firstname, DIR.lastname, NVL(DIR.email,'') AS email, NVL(DIR.phone,'') AS phone, CASE WHEN FF.host_id IS NOT NULL THEN 'Y' ELSE 'N' END AS isfreshmantransfer, CASE WHEN ATH.id IS NOT NULL THEN 'Y' ELSE 'N' END AS is_athlete,
                    CASE WHEN UBAL.hld_no IS NOT NULL THEN 'Y' ELSE 'N' END AS has_ubal, CASE TRIM(UPPER(NVL(SSR.bldg,''))) WHEN 'OFF' THEN 'O' WHEN 'CMTR' THEN 'C' WHEN '' THEN '?' ELSE 'R' END AS resident_commuter,
                    NVL(STAT.cum_earn_hrs, 0) AS earned_hours,
                    --Student Accounts
                    CASE
                        WHEN    NVL(SAR.pay_opt, '') LIKE 'Optn%'    THEN    'Y'
                        WHEN    NVL(SAR.pay_opt, '') LIKE 'Failed%'    THEN    'Y'
                        WHEN    NVL(SAR.pay_opt, '') LIKE 'Other%'    THEN    'Y'
                                                                    ELSE    'N'
                    END AS payment_options_form,
                    --CASE WHEN NVL(SSR.meal_plan_type,'') IN ('I','P') THEN 'Y' ELSE 'N' END AS room_and_board,
                    'N' AS room_and_board,
                    CASE
                        WHEN SAR.fin_clr = 'Y' THEN 'Y'
                        WHEN SAR.fin_clr = 'N' THEN 'N'
                        ELSE 'N'
                    END AS financial_clearance,
                    --Financial Aid
                    CASE TRIM(NVL(EC.stat,'')) WHEN 'E' THEN 'N' WHEN 'C' THEN 'Y' ELSE 'W' END AS entrance_counseling, CASE TRIM(NVL(PLEC.stat,'')) WHEN 'E' THEN 'N' WHEN 'C' THEN 'Y' ELSE 'W' END AS perkins_loan_entrance,
                    CASE TRIM(NVL(MPN.stat,'')) WHEN 'E' THEN 'N' WHEN 'C' THEN 'Y' ELSE 'W' END AS Perkins_Loan_master, CASE TRIM(NVL(VW.stat,'')) WHEN 'E' THEN 'N' WHEN 'C' THEN 'Y' ELSE 'W' END AS verification_worksheet,
                    CASE TRIM(NVL(SL.stat,'')) WHEN 'E' THEN 'N' WHEN 'C' THEN 'Y' ELSE 'W' END AS Stafford_Loan, CASE WHEN NVL(MD.missing,0) = 0 THEN 'Y' ELSE 'N' END AS No_Missing_Documents,
                    --Campus Nurse
                    CASE WHEN NVL(SMM.sitrep, 0) = 1 THEN 'Y' ELSE 'N' END AS medical_forms,
                    --Registrar
                    CASE WHEN TRIM(PROF.priv_code) = 'FERP' THEN 'Do not display' ELSE '' END AS ferpa_release, '' AS verify_address, '' AS verify_majors, '' AS distribute_schedule,
                    CASE
                        --If the student has earned less than 87 credits they are exempt from having to fill out the graduation candidacy form
                        WHEN    NVL(CRDTS.cum_earn_hrs,0) < 87    THEN    'W'
                        WHEN    GW.id    IS NOT    NULL            THEN    'Y'
                                                                ELSE
                                                                    CASE
                                                                        WHEN    SUC.student_id    IS NOT    NULL    THEN    '?'
                                                                                                                ELSE    'N'
                                                                    END
                    END AS confirm_graduate_status,
                    --Student ID
                    CASE WHEN PD.cx_id IS NOT NULL THEN 'Y' ELSE 'N' END AS has_id,
                    --Dean of Students
                    '' AS community_code, '' AS housing_survey,
                    --Security
                    CASE WHEN PRK.carthage_id IS NOT NULL THEN 'Y' ELSE 'N' END AS parking_permit
                FROM
                    directory_vw    DIR    LEFT JOIN    (
                                                        SELECT
                                                            host_id
                                                        FROM
                                                            jenzcst_rec
                                                        WHERE
                                                            status_code    IN    ('PFF','PFT')
                                                    )                FF        ON    DIR.id            =    FF.host_id
                                        LEFT JOIN    (
                                                        SELECT
                                                            involve_rec.id
                                                        FROM
                                                            involve_rec    INNER JOIN    invl_table    ON    involve_rec.invl        =    invl_table.invl
                                                                                                AND    invl_table.sanc_sport    =    'Y'
                                                        WHERE
                                                            involve_rec.yr        =    YEAR(TODAY)
                                                        AND
                                                            involve_rec.sess    =    'RA'
                                                        GROUP BY
                                                            involve_rec.id
                                                    )                ATH        ON    DIR.id            =    ATH.id
                                        LEFT JOIN    hold_rec        UBAL    ON    DIR.id            =    UBAL.id
                                                                            AND    UBAL.hld        =    'UBAL'
                                                                            AND    TODAY        BETWEEN    UBAL.beg_date AND NVL(UBAL.end_date, TODAY)
                                        LEFT JOIN    profile_rec        PROF    ON    DIR.id            =    PROF.id
                                       LEFT JOIN    stu_acad_rec    SAR        ON    DIR.id            =    SAR.id
                                                                            AND    SAR.yr            =    {0}
                                                                            AND    SAR.sess        =    '{1}'
                                        LEFT JOIN    stu_stat_rec    STAT    ON    DIR.id            =    STAT.id
                                                                            AND    STAT.prog        =    'UNDG'
                                        LEFT JOIN    stu_serv_rec    SSR        ON    DIR.id            =    SSR.id
                                                                            AND    SSR.yr            =    {0}
                                                                            AND    SSR.sess        =    '{1}'
                                        --Financial Aid - Entrance counseling
                                        LEFT JOIN    (
                                                        SELECT    id, stat    FROM    ctc_rec    WHERE    tick    =    (SELECT MAX(tick) FROM ctc_rec WHERE tick LIKE 'FY%') AND stat in ('E','C')    AND resrc    =    'FALNEC'
                                                    )                EC        ON    DIR.id            =    EC.id
                                        --Financial Aid - Perkins Loan entrance counseling
                                        LEFT JOIN    (
                                                        SELECT    id, stat    FROM    ctc_rec    WHERE    tick    =    (SELECT MAX(tick) FROM ctc_rec WHERE tick LIKE 'FY%') AND stat in ('E','C')    AND resrc    =    'FAPKEI'
                                                    )                PLEC    ON    DIR.id            =    PLEC.id
                                        --Financial Aid - Perkins Loan master promissory note
                                        LEFT JOIN    (
                                                        SELECT    id, stat    FROM    ctc_rec    WHERE    tick    =    (SELECT MAX(tick) FROM ctc_rec WHERE tick LIKE 'FY%') AND stat in ('E','C')    AND resrc    =    'FAPKMPN'
                                                    )                MPN        ON    DIR.id            =    MPN.id
                                        --Financial Aid - Verification worksheet
                                        LEFT JOIN    (
                                                        SELECT    id, stat    FROM    ctc_rec    WHERE    tick    =    (SELECT MAX(tick) FROM ctc_rec WHERE tick LIKE 'FY%') AND stat in ('E','C')    AND resrc    IN    ('INDVERIF','FAVRWK')
                                                    )                VW        ON    DIR.id            =    VW.id
                                        --Financial Aid - Stafford Loan
                                        LEFT JOIN    (
                                                        SELECT    id, stat    FROM    ctc_rec    WHERE    tick    =    (SELECT MAX(tick) FROM ctc_rec WHERE tick LIKE 'FY%') AND stat in ('E','C')    AND resrc    =    'FADLMPN'
                                                    )                SL        ON    DIR.id            =    SL.id
                                        --Financial Aid - Missing Documents
                                        LEFT JOIN    (
                                                        SELECT
                                                            ctc_rec.id, COUNT(*) AS missing
                                                        FROM
                                                            ctc_rec INNER JOIN ctc_table    ON    TRIM(ctc_rec.resrc)    =    TRIM(ctc_table.ctc)
                                                                                            AND    TRIM(ctc_rec.tick)    =    TRIM(ctc_table.tick)
                                                        WHERE
                                                            ctc_rec.tick            MATCHES    'FY*'
                                                        AND
                                                            ctc_rec.stat            =        'E'
                                                        AND
                                                            ctc_table.web_display    =        'Y'
                                                        AND
                                                            ctc_table.rte            =        'I'
                                                        GROUP BY
                                                            ctc_rec.id
                                                    )                MD        ON    DIR.id            =    MD.id
                                        --Health insurance
                                        LEFT JOIN    cc_student_medical_manager    SMM    ON    DIR.id    =    SMM.college_id
                                                                                    AND    SMM.created_at    >    TO_DATE('{0}-06-01', '%Y-%m-%d')
                                        --Registrar - credit threshold for graduation candidacy
                                        LEFT JOIN    stu_stat_rec    CRDTS    ON    DIR.id            =    CRDTS.id
                                                                            AND    CRDTS.prog        =    'UNDG'
                                        --Registrar - confirm graduation candidacy
                                        LEFT JOIN    gradwalk_rec    GW        ON    DIR.id            =    GW.id
                                                                            AND    GW.grad_yr        >=    {0}
                                        --Registrar - confirm graduation candidacy
                                        LEFT JOIN    cc_stg_undergrad_candidacy    SUC    ON    DIR.id            =    SUC.student_id
                                                                                    AND    SUC.grad_yr        >=    {0}
                                        --Student ID
                                        LEFT JOIN    provsndtl_rec    PD        ON    DIR.id            =    PD.cx_id
                                                                            AND    PD.provsystm    =    'Lenel'
                                                                            AND    PD.subsys        =    'MSTR'
                                        --Parking permit
                                        LEFT JOIN    (
                                                        SELECT
                                                            CPVR.carthage_id
                                                        FROM
                                                            cc_prkg_assign_rec    CPAR    INNER JOIN    cc_prkg_vehicle_rec    CPVR    ON    CPAR.veh_no    =    CPVR.veh_no
                                                        WHERE
                                                            CPAR.stat            =    'A'
                                                        AND
                                                            TODAY            BETWEEN    CPAR.assign_begin AND NVL(CPAR.assign_end, TODAY)
                                                        GROUP BY
                                                            CPVR.carthage_id
                                                    )                PRK        ON    DIR.id            =    PRK.carthage_id
                WHERE
                    DIR.class_year IN ('FF','FR','FN','JR','PFF','PTR','SO','SR','UT')
                AND
                    NVL(SAR.subprog,'') IN  ('TRAD','TRAP')
                GROUP BY
                    id, firstname, lastname, email, phone, isfreshmantransfer, has_ubal, resident_commuter, earned_hours, payment_options_form, financial_clearance, room_and_board, entrance_counseling, perkins_loan_entrance,
                    perkins_loan_master, verification_worksheet, stafford_loan, no_missing_documents, medical_forms, ferpa_release, verify_address, verify_majors, distribute_schedule,
                    confirm_graduate_status, has_id, community_code, housing_survey, parking_permit, is_athlete
                ORDER BY
                    lastname, firstname
            ", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            try
            {
                dtView = cxConn.ConnectToERP(sqlView, ref exView);
                if (exView != null) { throw exView; }
            }
            catch (Exception ex)
            {
                FormatException("Unable to retrieve CX student progress", ex, null, true);
            }
            finally
            {
                if (cxConn.IsNotClosed()) { cxConn.Close(); }
            }
            return dtView;
        }

        [Obsolete]
        public DataTable GetStudentProgress()
        {
            DataTable dtSP = null;
            //Exception exSP = null;
            //string sqlSP = "SELECT CAST(U.HostID AS INT) AS HostID, OT.ViewColumn, SP.* FROM FWK_User U INNER JOIN CI_StudentProgress SP ON U.ID = SP.UserID INNER JOIN CI_OfficeTask OT ON SP.TaskID = OT.TaskID WHERE SP.Yr = ? AND SP.Sess = ? AND HostID IS NOT NULL";
            //List<OdbcParameter> paramSP = new List<OdbcParameter>()
            //{
            //      new OdbcParameter("year", helper.ACTIVE_YEAR)
            //    , new OdbcParameter("session", helper.ACTIVE_SESSION)
            //};

            //OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
            //try
            //{
            //    dtSP = jicsConn.ConnectToERP(sqlSP, ref exSP, paramSP);
            //    if (exSP != null) { throw exSP; }
            //}
            //catch (Exception ex)
            //{
            //    FormatException("Unable to retrieve student progress from ICS_NET", ex, null, true);
            //}
            //finally
            //{
            //    if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            //}
            return dtSP;
        }

        [Obsolete]
        public DataTable GetMergedView()
        {
            DataTable dtCX = GetCXView();
            DataTable dtJICS = GetStudentProgress();

            if (dtCX != null && dtCX.Rows.Count > 0 && dtJICS != null && dtJICS.Rows.Count > 0)
            {
                foreach (DataRow dr in dtJICS.Rows)
                {
                    string cxID = dr["HostID"].ToString();
                    int hostID = 0;
                    CheckInTaskStatus derivedStatus = CheckInTaskStatus.No;

                    if (!String.IsNullOrWhiteSpace(cxID) && int.TryParse(cxID, out hostID))
                    {
                        DataRow drCX = dtCX.AsEnumerable().FirstOrDefault(row => row.Field<int>("id") == hostID);

                        bool doesCxRowExist = drCX != null;

                        if (doesCxRowExist)
                        {
                            CheckInTaskStatus cxStatus = helper.CreateTaskStatus(drCX == null ? CheckInTaskStatus.No.ToDescriptionString() : drCX.Field<string>(dr["ViewColumn"].ToString()));
                            CheckInTaskStatus jicsStatus = helper.CreateTaskStatus(dr["TaskStatus"].ToString());

                            if (jicsStatus == CheckInTaskStatus.Yes || cxStatus == CheckInTaskStatus.Yes)
                            {
                                derivedStatus = CheckInTaskStatus.Yes;
                            }
                            else if (jicsStatus == cxStatus)
                            {
                                derivedStatus = jicsStatus;
                            }
                            else if (jicsStatus == CheckInTaskStatus.Waived || (cxStatus == CheckInTaskStatus.Waived && jicsStatus == CheckInTaskStatus.Pending))
                            {
                                derivedStatus = CheckInTaskStatus.Waived;
                            }
                            else if ((jicsStatus == CheckInTaskStatus.Pending && cxStatus == CheckInTaskStatus.No) || (cxStatus == CheckInTaskStatus.Pending && jicsStatus == CheckInTaskStatus.No))
                            {
                                derivedStatus = CheckInTaskStatus.Pending;
                            }

                            dtCX.AsEnumerable().First(cx => cx.Field<int>("id") == hostID).SetField<string>(dr["ViewColumn"].ToString(), derivedStatus.ToDescriptionString());
                        }
                    }
                }
            }
            return dtCX;
        }

        [Obsolete]
        public DataTable StudentProgressCounts()
        {
            DataTable dtMerged = GetMergedView();
            dtMerged.Columns.AddRange(new DataColumn[]{
                  new DataColumn("completed_task_count", typeof(int))
                , new DataColumn("waived_task_count", typeof(int))
            });

            List<string> taskList = GetTaskViewColumns();

            for (int ii = 0; ii < dtMerged.Rows.Count; ii++)
            {
                int completedTasks = 0, waivedTasks = 0;
                foreach (string task in taskList)
                {
                    completedTasks += dtMerged.Rows[ii][task].ToString() == CheckInTaskStatus.Yes.ToDescriptionString() ? 1 : 0;
                    waivedTasks += dtMerged.Rows[ii][task].ToString() == CheckInTaskStatus.Waived.ToDescriptionString() ? 1 : 0;
                }
                dtMerged.Rows[ii]["completed_task_count"] = completedTasks;
                dtMerged.Rows[ii]["waived_task_count"] = waivedTasks;
            }

            return dtMerged;
        }

        [Obsolete]
        public DataTable GetCompletedStudents()
        {
            DataTable dtComplete = GetMergedView();
            try
            {
                List<string> viewColumns = GetTaskViewColumns();
                List<string> completeStatus = new List<string>()
                {
                      CheckInTaskStatus.Yes.ToDescriptionString()
                    , CheckInTaskStatus.Waived.ToDescriptionString()
                };

                foreach (string viewColumn in viewColumns)
                {
                    List<DataRow> drComplete = dtComplete.AsEnumerable().Where(row => completeStatus.Contains(row.Field<string>(viewColumn))).ToList();
                    if (drComplete.Count == 0)
                    {
                        dtComplete.Rows.Clear();
                        break;
                    }
                    else
                    {
                        dtComplete = drComplete.CopyToDataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                FormatException("Error while deriving the students who have completed check-in", ex);
            }

            return dtComplete;
        }

        [Obsolete]
        public DataTable GetIncompleteStudents()
        {
            DataTable dtAllStudents = GetMergedView(),
                dtCompleteStudents = GetCompletedStudents();

            if (dtCompleteStudents != null && dtCompleteStudents.Rows.Count > 0)
            {
                List<int> completedIDs = dtCompleteStudents.AsEnumerable().Select(complete => complete.Field<int>("id")).ToList();

                List<DataRow> drIncomplete = dtAllStudents.AsEnumerable().Where(row => !completedIDs.Contains(row.Field<int>("id"))).ToList();
                return drIncomplete.Count == 0 ? new DataTable() : drIncomplete.CopyToDataTable();
            }
            return dtAllStudents;
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
