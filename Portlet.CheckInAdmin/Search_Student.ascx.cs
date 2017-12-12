using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Jenzabar.Common.Globalization;
using Jenzabar.Common.Web.UI.Controls;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;
using Portlet.CheckInStudent;
using CUS.OdbcConnectionClass3;

namespace Portlet.CheckInAdmin
{
    public partial class Search_Student : PortletViewBase
    {
        Helper helper = new Helper();
        CheckInAdminHelper ciHelper = new CheckInAdminHelper();

        protected override void OnInit(EventArgs e)
        {
            if (this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA] != null)
            {
                if (this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA].ToString() == this.txtSearch.Text)
                {
                    this.txtSearch.Text = this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA].ToString();
                    //this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA] = null;
                }
                //else
                //{
                //    this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA] = this.txtSearch.Text;
                //}
            }

            if (this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_RESULTS] != null)
            {
                gvSearchResults.DataSource = (this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_RESULTS] as DataTable);
                gvSearchResults.DataBind();
            }

            if (this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID] != null)
            {
                this.ParentPortlet.NextScreen("Detail_Student");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtSearch.Text))
            {
                this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_CRITERIA] = this.txtSearch.Text;

                //Use a RegEx to determine if the search criteria is a valid ID format (4-9 digits)
                bool isID = Regex.IsMatch(this.txtSearch.Text, @"^\d{4,9}$");

                string whereClause = isID ? String.Format("AND CAST(U.HostID AS INT) = {0}", txtSearch.Text) : String.Format("AND LOWER(U.LastName) LIKE LOWER('{0}%')", txtSearch.Text);
                string sqlSearch = String.Format(@"
	                SELECT
		                U.LastName, U.FirstName, CAST(U.HostID AS INT) AS HostID, U.Email, SMD.IsCheckedIn, SMD.IsActive, SMD.FirstAccess, SMD.LastAccess, SMD.UserID, SMD.ActiveYear, SMD.ActiveSession
	                FROM
		                CI_StudentMetaData	SMD	INNER JOIN	FWK_User	U	ON	SMD.UserID	=	U.ID
	                WHERE
		                SMD.ActiveYear		=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveYear')
	                AND
		                SMD.ActiveSession	=	(SELECT [Value] FROM FWK_ConfigSettings WHERE Category = 'C_CheckIn' AND [Key] = 'ActiveSession')
	                {0}
                    ORDER BY
                        U.LastName, U.FirstName
                ", whereClause);

                try
                {
                    #region Original Search
                    //DataTable dtResults = ciHelper.GetMergedView();
                    //List<DataRow> drResults = new List<DataRow>() { };
                    //if (dtResults != null && dtResults.Rows.Count > 0)
                    //{
                    //    if (isID)
                    //    {
                    //        drResults = dtResults.AsEnumerable().Where(row => row.Field<int>("id") == int.Parse(this.txtSearch.Text)).ToList();
                    //    }
                    //    else
                    //    {
                    //        drResults = dtResults.AsEnumerable().Where(row => row.Field<string>("lastname").ToLower().StartsWith(this.txtSearch.Text.ToLower())).ToList();
                    //    }

                    //    this.lblSearchResults.Text = this.lblSearchResults2.Text = drResults.Count.ToString();
                    //    this.lblSearchResults.Visible = this.lblSearchResults2.Visible = true;
                    //    if (drResults.Count == 1)
                    //    {
                    //        this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID] = this.txtSearch.Text;
                    //        this.ParentPortlet.NextScreen("Detail_Student");
                    //    }
                    //    else
                    //    {
                    //        this.lblSearchResults.Text = this.lblSearchResults2.Text = String.Format("Found {0} matches", drResults.Count.ToString());
                    //        gvSearchResults.DataSource = drResults.Count > 1 ? drResults.CopyToDataTable() : new DataTable();
                    //        gvSearchResults.DataBind();
                    //    }
                    //}
                    #endregion

                    OdbcConnectionClass3 jicsConn = helper.CONNECTION_JICS;
                    DataTable dtSearch = null;
                    Exception exSearch = null;

                    dtSearch = jicsConn.ConnectToERP(sqlSearch, ref exSearch);
                    if (exSearch != null) { throw exSearch; }
                    if (dtSearch != null)
                    {
                        //If the search results only return a single record, go immediately to the detail view
                        if (dtSearch.Rows.Count == 1)
                        {
                            this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID] = this.txtSearch.Text;
                            this.ParentPortlet.NextScreen("Detail_Student");
                        }
                        else
                        {
                            this.lblSearchResults.Text = this.lblSearchResults2.Text = String.Format("Found {0} matches", dtSearch.Rows.Count.ToString());
                            this.lblSearchResults.Visible = this.lblSearchResults2.Visible = true;
                            gvSearchResults.DataSource = dtSearch;
                            gvSearchResults.DataBind();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.ParentPortlet.ShowFeedback(FeedbackType.Error, ciHelper.FormatException("An exception occurred while filtering search results", ex));
                }
            }
            else
            {
                this.ParentPortlet.ShowFeedback(FeedbackType.Message, "Please enter either a last name or student ID");
            }
        }

        protected void lbStudentDetail_Click(object sender, EventArgs e)
        {
            LinkButton lbStudent = (sender as LinkButton);
            GridViewRow row = (lbStudent.NamingContainer as GridViewRow);
            string studentID = gvSearchResults.DataKeys[row.RowIndex].Value.ToString();
            this.ParentPortlet.PortletViewState[ciHelper.VIEWSTATE_SEARCH_STUDENTID] = studentID;
            this.ParentPortlet.NextScreen("Detail_Student");
        }

        /*
        protected void gvSearchResults_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                DataTable dtTasks = ciHelper.GetTasks();

                GridViewRow trOffice = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);

                TableHeaderCell thName = new TableHeaderCell();
                trOffice.Cells.Add(thName);

                List<string> offices = dtTasks.AsEnumerable().Select(row => row.Field<string>("OfficeName")).Distinct().ToList();
                foreach (string office in offices)
                {
                    TableHeaderCell thOffice = new TableHeaderCell();
                    thOffice.Text = office;

                    List<string> tasks = dtTasks.AsEnumerable().Where(row => row.Field<string>("OfficeName") == office).Select(row => row.Field<string>("TaskName")).ToList();
                    thOffice.ColumnSpan = tasks.Count;
                    trOffice.Cells.Add(thOffice);
                }
                gvSearchResults.Controls[0].Controls.AddAt(0, trOffice);
            }
        }
        */
        protected void gvSearchResults_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lbName = (LinkButton)e.Row.FindControl("lbStudent");
                DataRowView drvRow = e.Row.DataItem as DataRowView;

                lbName.Text = String.Format("{0} {1} ({2})", drvRow["firstname"].ToString(), drvRow["lastname"].ToString(), drvRow["HostID"].ToString());

                e.Row.Cells[0].Controls.Add(lbName);
            }
        }
        /*
        protected void gvSearchResults_Init(object sender, EventArgs e)
        {
            DataTable dtTasks = ciHelper.GetTasks();
            foreach (DataRow task in dtTasks.Rows)
            {
                BoundField tdTask = new BoundField();
                tdTask.HeaderText = task["TaskName"].ToString();
                tdTask.DataField = task["ViewColumn"].ToString();
                gvSearchResults.Columns.Add(tdTask);
            }
        }
        */

        protected void lbFacetSearch_Click(object sender, EventArgs e)
        {
            this.ParentPortlet.ChangeScreen("Facet_Search");
        }
    }
}