<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Search_Student.ascx.cs" Inherits="Portlet.CheckInAdmin.Search_Student" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<asp:LinkButton ID="lbFacetSearch" runat="server" Text="Look up students by progress" OnClick="lbFacetSearch_Click" />


<script type="text/javascript">
    $(function () {
        assignClassByStatus('Y', 'complete');
        assignClassByStatus('N', 'incomplete');
        assignClassByStatus('P', 'pending');
        assignClassByStatus('W', 'waived');
    });

    function assignClassByStatus(statusCode, className) {
        $('.nameSearchResults tr td').filter(function () { return $(this).text() == statusCode; }).addClass(className);
    }
</script>

<asp:Panel ID="panelSearchForm" runat="server" CssClass="searchNameID">
    <label for="<%= this.txtSearch.ClientID %>">Search:</label>
    <asp:TextBox ID="txtSearch" runat="server" />
    <asp:Button ID="btnSearch" runat="server" Text="Find Student" OnClick="btnSearch_Click" />
</asp:Panel>


<asp:Label ID="lblSearchResults" runat="server" Visible="false" />

<asp:GridView ID="gvSearchResults" runat="server" CellSpacing="0" CssClass="nameSearchResults" OnInit="gvSearchResults_Init" OnRowCreated="gvSearchResults_RowCreated" OnRowDataBound="gvSearchResults_RowDataBound" AutoGenerateColumns="false" DataKeyNames="id">
    <Columns>
        <asp:TemplateField HeaderText="Name">
            <ItemTemplate>
                <asp:LinkButton ID="lbStudent" runat="server" OnClick="lbStudentDetail_Click" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

<asp:Label ID="lblSearchResults2" runat="server" Visible="false" />