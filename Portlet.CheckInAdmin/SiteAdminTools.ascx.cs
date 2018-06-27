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

namespace Portlet.CheckInAdmin
{
    public partial class SiteAdminTools : PortletViewBase
    {
        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsFirstLoad)
            {
                BindGrid();

                OfficeTaskInit();
                RolloverInit();
                StudentMetaDataInit();
            }
        }

        private void BindGrid()
        {
            OdbcConnectionClass3 jicsConn = helper.CONNECTION_SP;
            DataTable dtConfig = null;
            Exception exConfig = null;
            string sqlConfig = String.Format("SELECT ID, [Key] AS ConfigKey, [Value] AS ConfigValue, DefaultValue AS ConfigDefaultValue FROM FWK_ConfigSettings WHERE Category = ? ORDER BY [Key]");
            List<OdbcParameter> paramConfig = new List<OdbcParameter>()
            {
                new OdbcParameter("checkInCategory", "C_CheckIn")
            };

            try
            {
                dtConfig = jicsConn.ConnectToERP(sqlConfig, ref exConfig, paramConfig);
                if (exConfig != null) { throw exConfig; }
                gvConfigSettings.DataSource = dtConfig;
                gvConfigSettings.DataBind();
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Unable to load configuration data", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("Unable to load configuration data", ex, null, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlConfig)
                );
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }
        }

        private DataTable GetOfficeTaskSession()
        {
            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            DataTable dtOfficeTaskSession = null;
            Exception exOfficeTaskSession = null;
            string sqlOfficeTaskSession = String.Format(@"
                SELECT
                    O.OfficeName, OT.TaskName, O.OfficeName + ' - ' + OT.TaskName AS OfficeTaskLabel, OT.TaskID, OTS.IsRollover, OTS.RolloverYear, OTS.RolloverSession, OTS.OfficeTaskSessionID
                FROM
                    CI_OfficeTaskSession    OTS INNER JOIN  CI_OfficeTask   OT  ON  OTS.OfficeTaskID    =   OT.TaskID
                                                INNER JOIN  CI_Office       O   ON  OT.OfficeID         =   O.OfficeID
                WHERE
                    ActiveYear      =   {0}
                AND
                    ActiveSession   =   '{1}'
                ORDER BY
                    O.Sequence, O.OfficeName, OT.Sequence, OT.TaskName
            ", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            try
            {
                dtOfficeTaskSession = jicsSpConn.ConnectToERP(sqlOfficeTaskSession, ref exOfficeTaskSession);
                if (exOfficeTaskSession != null) { throw exOfficeTaskSession; }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while retrieving OfficeTaskSession in session initialization interface.", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("Error while retrieving OfficeTaskSession in session initialization interface.", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlOfficeTaskSession)
                );
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
            return dtOfficeTaskSession;
        }

        #region Panel Init

        private void OfficeTaskInit()
        {
            this.lblSession1.Text = helper.ACTIVE_SESSION_TEXT;
            this.lblYear1.Text = helper.ACTIVE_YEAR.ToString();
            this.lblOTSCount.Text = GetOfficeTaskSession().Rows.Count.ToString();
            //Add range of years
            this.ddlInitRolloverYear.Items.AddRange(Enumerable.Range(helper.ACTIVE_YEAR, 4).Select(li => new ListItem(li.ToString())).OrderByDescending(li => li.Value).ToArray());

            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            DataTable dtOfficeTaskSession = null;
            Exception exOfficeTaskSession = null;

            string sqlOfficeTaskSession = String.Format(@"
                SELECT
                    O.OfficeID, O.OfficeName, OT.TaskID, OT.TaskName, OT.ViewColumn, O.OfficeName + ' - ' + OT.TaskName AS OfficeTaskLabel
                FROM
                    CI_OfficeTask   OT  INNER JOIN  CI_Office				O   ON  OT.OfficeID			=   O.OfficeID
                                        LEFT JOIN	CI_OfficeTaskSession	OTS	ON	OT.TaskID			=	OTS.OfficeTaskID
                                                                                AND	OTS.ActiveYear		=	{0}
                                                                                AND	OTS.ActiveSession	=	'{1}'
                WHERE
                    OTS.OfficeTaskSessionID IS  NULL
                ORDER BY
                    O.Sequence, O.OfficeName, OT.Sequence, OT.TaskName"
            , helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            try
            {

                dtOfficeTaskSession = jicsSpConn.ConnectToERP(sqlOfficeTaskSession, ref exOfficeTaskSession);
                if (exOfficeTaskSession != null) { throw exOfficeTaskSession; }
                if (dtOfficeTaskSession != null && dtOfficeTaskSession.Rows.Count > 0)
                {
                    this.lblOTCount.Text = dtOfficeTaskSession.Rows.Count.ToString();
                    this.cblOfficeTaskSession.DataSource = dtOfficeTaskSession;
                    this.cblOfficeTaskSession.DataBind();
                }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("<p>Could not load Office Task Session initialization.</p>", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("<p>Could not load Office Task Session initialization.</p>", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlOfficeTaskSession));
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
        }

        private void RolloverInit()
        {
            try
            {
                this.ddlRolloverTaskYear.Items.AddRange(Enumerable.Range(helper.ACTIVE_YEAR, 4).Select(li => new ListItem(li.ToString())).OrderByDescending(li => li.Value).ToArray());
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not set range for rollover task year", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("Could not set range for rollover task year", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools));
            }

            try
            {
                DataTable dtOfficeTaskSession = GetOfficeTaskSession();
                if (dtOfficeTaskSession != null && dtOfficeTaskSession.Rows.Count > 0)
                {
                    this.cblRolloverTask.DataSource = dtOfficeTaskSession;
                    this.cblRolloverTask.DataBind();

                    List<string> rolloverTaskIDs = dtOfficeTaskSession.AsEnumerable().Where(row => row.Field<bool>("IsRollover") == true).Select(row => row.Field<Guid>("OfficeTaskSessionID").ToString()).ToList();
                    foreach (string taskID in rolloverTaskIDs)
                    {
                        this.cblRolloverTask.Items.FindByValue(taskID).Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not load checkbox list of officetasksession", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException("Could not load checkbox list of officetasksession", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools));
            }
        }

        private void StudentMetaDataInit()
        {
            string sqlStudents = "";
            try
            {
                if (helper.ACTIVE_SESSION == "RA")
                {
                    sqlStudents = String.Format("EXECUTE PROCEDURE ci_get_students_fall({0})", helper.ACTIVE_YEAR);
                }
                else if (helper.ACTIVE_SESSION == "RC")
                {
                    sqlStudents = String.Format("EXECUTE PROCEDURE ci_get_students_spring({0}, 'RA', {1}, 'RC')", helper.ACTIVE_YEAR - 1, helper.ACTIVE_YEAR);
                }
                else
                {
                    throw new Exception(String.Format("Unknown session: {0}; could not identify procedure to retrieve students"));
                }

                OdbcConnectionClass3 cxSpConn = helper.CONNECTION_CX_SP;
                DataTable dtStudents = null;
                Exception exStudents = null;
                try
                {
                    dtStudents = cxSpConn.ConnectToERP(sqlStudents, ref exStudents);
                    if (exStudents != null) { throw exStudents; }
                    this.lblSMDCount.Text = dtStudents == null ? "0" : dtStudents.Rows.Count.ToString();
                }
                catch (Exception ex)
                {
                    //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not retrieve students to update meta data", ex));
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                        ciHelper.FormatException("Could not retrieve students to update meta data", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlStudents));
                }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while initializing student meta data", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while initializing student meta data", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools));
            }
        }

        #endregion

        #region Grid Events

        protected void gvConfigSettings_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvConfigSettings.EditIndex = -1;
            BindGrid();
        }

        protected void gvConfigSettings_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvConfigSettings.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvConfigSettings_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow gvr = gvConfigSettings.Rows[e.RowIndex];
            string configID = gvConfigSettings.DataKeys[e.RowIndex].Value.ToString();
            string configKey = (gvr.FindControl("txtKey") as TextBox).Text;
            string configValue = (gvr.FindControl("txtValue") as TextBox).Text;

            string sqlUpdate = String.Format("UPDATE FWK_ConfigSettings SET [Value] = ? WHERE ID = ?", configValue, configID);
            List<OdbcParameter> paramUpdate = new List<OdbcParameter>()
            {
                  new OdbcParameter("value", configValue)
                , new OdbcParameter("configID", configID)
            };
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, sqlUpdate);

            OdbcConnectionClass3 jicsConn = helper.CONNECTION_SP;
            Exception exUpdate = null;
            try
            {
                jicsConn.ConnectToERP(sqlUpdate, ref exUpdate, paramUpdate);
                if (exUpdate != null) { throw exUpdate; }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Failed to update configuration settings", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Failed to update configuration settings", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlUpdate));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }

            gvConfigSettings.EditIndex = -1;
            BindGrid();
        }

        #endregion

        #region Button Click Events

        protected void btnAddOTS_Click(object sender, EventArgs e)
        {
            string rolloverYear = String.IsNullOrWhiteSpace(this.ddlInitRolloverYear.SelectedValue) ? "NULL" : this.ddlInitRolloverYear.SelectedValue;
            string rolloverSession = String.IsNullOrWhiteSpace(this.ddlInitRolloverSession.SelectedValue) ? "NULL" : this.ddlInitRolloverSession.SelectedValue;

            string selectedOfficeTasks = String.Join(",", this.cblOfficeTaskSession.Items.Cast<ListItem>().Where(li => li.Selected == true).Select(li => "'" + li.Value + "'"));
            string sqlInsert = String.Format(@"
                INSERT INTO CI_OfficeTaskSession (OfficeTaskID, ActiveYear, ActiveSession, IsRollover, RolloverYear, RolloverSession)
                SELECT
                    OT.TaskID, {0}, '{1}', 0, {2}, {3}
                FROM
                    CI_OfficeTask   OT
                WHERE
                    OT.TaskID   IN  ({4})",
            helper.ACTIVE_YEAR, helper.ACTIVE_SESSION, rolloverYear, rolloverSession, selectedOfficeTasks);

            OdbcConnectionClass3 jicsConn = helper.CONNECTION_SP;
            Exception exRollover = null;
            try
            {
                jicsConn.ConnectToERP(sqlInsert, ref exRollover);
                if (exRollover != null) { throw exRollover; }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while attempting to insert OfficeTaskSession records", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Error while attempting to insert OfficeTaskSession records", ex, null, null, null,
                    LogEventType.Error, LogScreen.SiteAdminTools, sqlInsert));
            }
            finally
            {
                if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
            }

            this.ParentPortlet.ShowFeedback(FeedbackType.Message, sqlInsert);
        }

        protected void btnAddRollover_Click(object sender, EventArgs e)
        {
            string feedback = "";

            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            Exception exReset = null;
            string rolloverYear = String.IsNullOrWhiteSpace(this.ddlRolloverTaskYear.SelectedValue) ? "NULL" : this.ddlRolloverTaskYear.SelectedValue;
            string rolloverSession = String.IsNullOrWhiteSpace(this.ddlRolloverTaskSession.SelectedValue) ? "NULL" : "'" + this.ddlRolloverTaskSession.SelectedValue + "'";

            string sqlReset = String.Format(@"
                UPDATE
                    CI_OfficeTaskSession
                SET
                      IsRollover      =   0
                    , RolloverYear    =   {0}
                    , RolloverSession =   {1}
                WHERE
                    ActiveYear      =   {2}
                AND
                    ActiveSession   =   '{3}'
            ", rolloverYear, rolloverSession, helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            feedback = String.Format("{0}<p>{1}</p>", feedback, sqlReset);

            try
            {
                jicsSpConn.ConnectToERP(sqlReset, ref exReset);
                if (exReset != null) { throw exReset; }

                string selectedOfficeTaskSession = String.Join(",", this.cblRolloverTask.Items.Cast<ListItem>().Where(li => li.Selected == true).Select(li => "'" + li.Value + "'"));
                if (!String.IsNullOrWhiteSpace(selectedOfficeTaskSession))
                {
                    Exception exUpdate = null;
                    string sqlUpdate = String.Format(@"
                        UPDATE
                            CI_OfficeTaskSession
                        SET
                            IsRollover  =   1
                        WHERE
                            OfficeTaskSessionID IN  ({0})
                    ", selectedOfficeTaskSession);

                    try
                    {
                        jicsSpConn.ConnectToERP(sqlUpdate, ref exUpdate);
                        if (exUpdate != null) { throw exUpdate; }
                    }
                    catch (Exception ex)
                    {
                        //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not update rollover data", ex));
                        this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not update rollover data", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlUpdate));
                    }

                    feedback = String.Format("{0}<p>{1}</p>", feedback, sqlUpdate);
                }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not reset rollover data", ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("Could not reset rollover data", ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlReset));
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, feedback);
        }

        protected void btnLoadSMD_Click(object sender, EventArgs e)
        {
            ciHelper.GenerateStudentMetaData();
        }

        protected void btnInitStudentProgress_Click(object sender, EventArgs e)
        {
            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            Exception exStudentProgress = null;
            string sqlStudentProgress = String.Format(@"EXECUTE CUS_spCheckIn_InitializeStudentProgress @intYear = {0}, @strSession = '{1}'", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            try
            {
                jicsSpConn.ConnectToERP(sqlStudentProgress, ref exStudentProgress);
                if (exStudentProgress != null) { throw exStudentProgress; }
            }
            catch (Exception ex)
            {
                //this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException(String.Format("<p>Could not initialize student progress</p><p>{0}</p>", sqlStudentProgress), ex));
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException(String.Format("<p>Could not initialize student progress</p><p>{0}</p>", sqlStudentProgress), ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlStudentProgress));
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
        }

        protected void btnProcessRollover_Click(object sender, EventArgs e)
        {
            OdbcConnectionClass3 jicsSpConn = helper.CONNECTION_SP;
            Exception exProcessRollover = null;
            string sqlProcessRollover = String.Format(@"EXECUTE CUS_spCheckIn_ProcessRollover @intYear = {0}, @strSession = '{1}'", helper.ACTIVE_YEAR, helper.ACTIVE_SESSION);

            try
            {
                jicsSpConn.ConnectToERP(sqlProcessRollover, ref exProcessRollover);
                if (exProcessRollover != null) { throw exProcessRollover; }
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error,
                    ciHelper.FormatException(String.Format("<p>Could not process rollover data</p><p>{0}</p>", sqlProcessRollover), ex, null, null, null, LogEventType.Error, LogScreen.SiteAdminTools, sqlProcessRollover));
            }
            finally
            {
                if (jicsSpConn.IsNotClosed()) { jicsSpConn.Close(); }
            }
        }

        #endregion
    }
}