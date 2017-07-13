using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenzabar.Common.Globalization;
using Jenzabar.Common.Web.UI.Controls;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web.UI;

namespace Portlet.CheckInAdmin
{
    public class CI_Admin : PortletBase, ICssProvider
    {
        protected override PortletViewBase GetCurrentScreen()
        {
            CheckInAdminHelper ciHelper = new CheckInAdminHelper();
            PortletViewBase screen = null;
            switch (this.CurrentPortletScreenName)
            {
                //case "Detail_Student":
                //    //screen = this.LoadPortletView(String.Format("Portlet.CheckInAdmin/{0}.ascx", this.CurrentPortletScreenName));
                //    screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Detail_Student.ascx");
                //    break;
                //case "Search_Student":
                //    //screen = this.LoadPortletView(String.Format("Portlet.CheckInAdmin/{0}.ascx", this.CurrentPortletScreenName));
                //    screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Search_Student.ascx");
                //    break;
                case "Dashboard":
                case "Facet_Search":
                case "Detail_Student":
                case "Search_Student":
                    screen = this.LoadPortletView(String.Format("ICS/Portlet.CheckInAdmin/{0}.ascx", this.CurrentPortletScreenName));
                    break;
                default:
                    //screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Search_Student.ascx");
                    screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Dashboard.ascx");
                    break;
            }
            return screen;
        }

        public string CssClass { get { return "checkInAdmin"; } }
        public string CssFileLocation { get { return "~/Portlets/CUS/ICS/Portlet.CheckInAdmin/CheckInAdmin.css"; } }
    }
}