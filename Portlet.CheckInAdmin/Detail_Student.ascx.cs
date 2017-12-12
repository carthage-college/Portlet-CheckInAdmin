using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class Detail_Student : PortletViewBase
    {
        OdbcConnectionClass3 cxConn = new OdbcConnectionClass3("ERPDataConnection.config");
        OdbcConnectionClass3 spConn = new OdbcConnectionClass3("JICSDataConnection.config", true);

        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            bool foundStudent = false;
            int studentID = 0;

            try
            {
                if (String.IsNullOrWhiteSpace(this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString()))
                {
                    //this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_MESSAGE] = String.Format("No ID (or invalid ID) was passed to the detail screen. Value of ID: '{0}'.", this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString());
                    //Return to Search_Student screen with error message
                    //this.ParentPortlet.NextScreen(ciHelper.VIEW_SEARCH);
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, String.Format("No ID (or invalid ID) was passed to the detail screen. Value of ID: '{0}'.", this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString()));
                    //return;
                }

                int.TryParse(this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString(), out studentID);
                this.ltlStudentID.Text = studentID.ToString();

                //Set flag for remainder of page
                foundStudent = studentID != 0;
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while retrieving the student's ID", ex));
            }

            if (foundStudent)
            {
                OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
                DataTable dtStudent = null;
                Exception exStudent = null;
                string sqlStudent = String.Format("SELECT FirstName, LastName FROM FWK_User WHERE CAST(HostID AS INT) = {0}", studentID);

                try
                {
                    dtStudent = jicsConn.ConnectToERP(sqlStudent, ref exStudent);
                    if (exStudent != null) { throw exStudent; }
                    if (dtStudent != null && dtStudent.Rows.Count > 0)
                    {
                        //this.ltlStudentName.Text = String.Format("{0} {1}", dtStudent.Rows[0]["FirstName"].ToString(), dtStudent.Rows[0]["LastName"].ToString());
                        this.shDetail.Text = String.Format("Student Detail View for {0} {1} (ID: {2})",
                            dtStudent.Rows[0]["FirstName"].ToString(), dtStudent.Rows[0]["LastName"].ToString(), studentID);
                    }
                }
                catch (Exception ex)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An error occurred while retrieving student name", ex));
                }
                finally
                {
                    if (jicsConn.IsNotClosed()) { jicsConn.Close(); }
                }

                LoadStudentProgress(studentID);
            }
        }

        protected void LoadStudentProgress(int studentID)
        {
            Exception exProgress = null;
            DataRow drCX = helper.GetCheckinRow(ref exProgress, studentID);

            try
            {
                DataTable dtJICS = null;
                Exception exJICS = null;
                string sqlJICS = String.Format(@"EXECUTE dbo.CUS_spCheckIn_AdminGetTasks @intHostID = {0}", studentID);
                dtJICS = spConn.ConnectToERP(sqlJICS, ref exJICS);
                if (exJICS != null) { throw exJICS; }

                for (int ii = 0; ii < dtJICS.Rows.Count; ii++)
                {
                    dtJICS.Rows[ii]["CX_Status"] = drCX[dtJICS.Rows[ii]["ViewColumn"].ToString()];
                    //If the status from CX is Yes or the status from JICS is non-existent, use the CX value. Otherwise, use the JICS value.
                    CheckInTaskStatus taskStatus = EnumUtil.GetEnumFromDescription<CheckInTaskStatus>(dtJICS.Rows[ii]["CX_Status"].ToString());
                    dtJICS.Rows[ii]["TaskStatus"] = taskStatus == CheckInTaskStatus.Yes || String.IsNullOrEmpty(dtJICS.Rows[ii]["JICS_Status"].ToString()) ? dtJICS.Rows[ii]["CX_Status"].ToString() : dtJICS.Rows[ii]["JICS_Status"].ToString();
                }

                dgTasks.DataSource = dtJICS;
                dgTasks.DataBind();
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An error occurred while retrieving the student's record", ex, null, true));
            }
            finally
            {
                if (cxConn.IsNotClosed()) { cxConn.Close(); }
            }
        }

        protected void dgTasks_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ((Button)e.Item.FindControl("btnStatusY")).CommandArgument =
                    ((Button)e.Item.FindControl("btnStatusN")).CommandArgument =
                    ((Button)e.Item.FindControl("btnStatusP")).CommandArgument =
                    ((Button)e.Item.FindControl("btnStatusW")).CommandArgument = dgTasks.DataKeys[e.Item.ItemIndex].ToString();

                //string taskStatus = DataBinder.Eval(e.Item.DataItem, "TaskStatus").ToString();
                DataRowView row = e.Item.DataItem as DataRowView;

                //Disable the button for the task's current state
                string buttonID = String.Format("btnStatus{0}", row["TaskStatus"].ToString());
                ((Button)e.Item.FindControl(buttonID)).CommandArgument = null;
                ((Button)e.Item.FindControl(buttonID)).Enabled = false;
                ((Button)e.Item.FindControl(buttonID)).CssClass += " activeStatus";

                if (row["TaskStatus"].ToString() == CheckInTaskStatus.Yes.ToDescriptionString())
                {
                    ((Button)e.Item.FindControl("btnStatusN")).Visible =
                    ((Button)e.Item.FindControl("btnStatusP")).Visible =
                    ((Button)e.Item.FindControl("btnStatusW")).Visible = false;
                }
            }
        }

        public void handleStatusChange(string taskID, string status)
        {
            //this.ParentPortlet.ShowFeedback(FeedbackType.Message, String.Format("UPDATE CI_OfficeTask SET TaskStatus = '{0}' WHERE TaskID = '{1}'", status, taskID));
            string feedbackUpdate = String.Format(@"EXECUTE CUS_spCheckIn_UpdateTask @uuidTaskID = '{0}', @strTaskStatus = '{1}', @uuidStatusUserID = '{2}', @intHostID = {3}",
                taskID, status, PortalUser.Current.Guid.ToString(),
                this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString());
            this.ParentPortlet.ShowFeedback(FeedbackType.Message, feedbackUpdate);

            int studentID = int.Parse(this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID].ToString());
            try
            {
                OdbcConnectionClass3 spConn = helper.CONNECTION_SP;
                Exception exUpdate = null;
                string sqlUpdate = String.Format("EXECUTE CUS_spCheckIn_UpdateTask @uuidTaskID = ?, @strTaskStatus = ?, @uuidStatusUserID = ?, @intHostID = {0}",
                    studentID);
                List<OdbcParameter> paramUpdate = new List<OdbcParameter>()
                {
                    new OdbcParameter("taskID", taskID)
                    , new OdbcParameter("status", status)
                    , new OdbcParameter("statusUserID", PortalUser.Current.Guid.ToString())
                };

                spConn.ConnectToERP(sqlUpdate, ref exUpdate, paramUpdate);
                if (exUpdate != null) { throw exUpdate; }
                
                //If the stored procedure executed successfully, reload the table to reflect the updated information
                LoadStudentProgress(studentID);
            }
            catch (Exception ex)
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while updating the status of this task", ex, null, true));
            }
            finally
            {
                if (spConn.IsNotClosed()) { spConn.Close(); }
            }
        }

        #region Event Handlers

        protected void btnBackToSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID] = null;
            this.ParentPortlet.PreviousScreen(ciHelper.VIEW_SEARCH);
        }

        protected void btnStatusY_Click(object sender, EventArgs e)
        {
            handleStatusChange((sender as Button).CommandArgument, CheckInTaskStatus.Yes.ToDescriptionString());
        }

        protected void btnStatusN_Click(object sender, EventArgs e)
        {
            handleStatusChange((sender as Button).CommandArgument, CheckInTaskStatus.No.ToDescriptionString());
        }

        protected void btnStatusP_Click(object sender, EventArgs e)
        {
            handleStatusChange((sender as Button).CommandArgument, CheckInTaskStatus.Pending.ToDescriptionString());
        }

        protected void btnStatusW_Click(object sender, EventArgs e)
        {
            handleStatusChange((sender as Button).CommandArgument, CheckInTaskStatus.Waived.ToDescriptionString());
        }

        #endregion
    }
}