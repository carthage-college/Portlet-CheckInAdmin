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
            try
            {
                screen = this.LoadPortletView(String.Format("ICS/Portlet.CheckInAdmin/{0}.ascx", this.CurrentPortletScreenName));
            }
            catch (Exception ex)
            {
                screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Dashboard.ascx");
            }
            //switch (this.CurrentPortletScreenName)
            //{
            //    case "Dashboard":
            //    case "Facet_Search":
            //    case "Detail_Student":
            //    case "Search_Student":
            //        screen = this.LoadPortletView(String.Format("ICS/Portlet.CheckInAdmin/{0}.ascx", this.CurrentPortletScreenName));
            //        break;
            //    default:
            //        //screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Search_Student.ascx");
            //        screen = this.LoadPortletView("ICS/Portlet.CheckInAdmin/Dashboard.ascx");
            //        break;
            //}
            return screen;
        }

        public string CssClass { get { return "checkInAdmin"; } }
        public string CssFileLocation { get { return "~/Portlets/CUS/ICS/Portlet.CheckInAdmin/CheckInAdmin.css"; } }
    }
}