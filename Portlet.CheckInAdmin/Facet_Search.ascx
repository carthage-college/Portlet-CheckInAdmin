<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Facet_Search.ascx.cs" Inherits="Portlet.CheckInAdmin.Facet_Search" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<style type="text/css">
    .hide {display:none;}
</style>
<script type="text/javascript">
    $(function () {
        $('tr.radioRowOffice td input[type="radio"]').click(function () {
            var officeID = $(this).parent('span').data('office');
            var status = $(this).parent('span').data('status');

            status = status == '*' ? '\\*' : status;
            $('td.radioRowTaskStatus span[data-office="' + officeID + '"]').removeProp('checked');
            $('td.radioRowTaskStatus span[data-office="' + officeID + '"][data-status="' + status + '"] input').prop({ 'checked': 'checked' });
        });

        $('tr.radioRowTask td input[type="radio"]').click(function () {
            var officeID = $(this).parent('span').data('office');
            var status = $(this).parent('span').data('status');

            $('td.radioRowOfficeStatus span[data-office="' + officeID + '"]').removeProp('checked')
            $('td.radioRowOfficeStatus span[data-office="' + officeID + '"][data-status="\\*"] input').prop({ 'checked': 'checked' });
        });

        $('.facetSearchResults tr:not(:first)').each(function (index, obj) {
            $rowCells = $(obj).find('td:not([class="basicInfo"])');
            var allComplete = $rowCells.length == $rowCells.filter(function () { return $(this).text() == 'Y' || $(this).text() == 'W'; }).length;
            //console.log('All complete is ' + allComplete + ' for row ' + index);
            $(obj).toggleClass('complete', allComplete);
        });

        assignClassByStatus('Y', 'complete');
        assignClassByStatus('N', 'incomplete');
        assignClassByStatus('P', 'pending');
        assignClassByStatus('W', 'waived');
    });

    function assignClassByStatus(statusCode, className) {
        $('.facetSearchResults tr td').filter(function () { return $(this).text() == statusCode; }).addClass(className);
    }
</script>

<asp:LinkButton ID="aNameSearch" runat="server" Text="Search by Student Name/ID" OnClick="aNameSearch_Click" />

<fieldset class="fieldsetFacetedSearch">
    <legend>Search Facets</legend>
    <asp:Panel ID="panelFacetedSearch" runat="server">
        <div class="facet">
            <asp:Label ID="lblStanding" runat="server" CssClass="facetLabel" Text="Standing:" />
            <asp:DropDownList ID="ddlStanding" runat="server" CssClass="facetOptions">
                <asp:ListItem Text="Any" Value="" />
                <asp:ListItem Text="Freshman/Transfer" Value="Y" />
                <asp:ListItem Text="Returning Student" Value="N" />
            </asp:DropDownList>
        </div>
        <div class="facet">
            <asp:Label ID="lblAthlete" runat="server" CssClass="facetLabel" Text="Athlete:" />
            <asp:ListBox ID="lbAthletics" runat="server" SelectionMode="Multiple">
            </asp:ListBox>
        </div>
        <div class="facet">
            <asp:Label ID="lblResidency" runat="server" CssClass="facetLabel" Text="Residency:" />
            <asp:CheckBoxList ID="cblResidency" runat="server" CssClass="facetOptions" RepeatDirection="Horizontal">
                <asp:ListItem Text="Resident" Value="R" />
                <asp:ListItem Text="Commuter" Value="C" />
                <asp:ListItem Text="Off-Campus" Value="O" />
            </asp:CheckBoxList>
        </div>
        <div class="facet">
            <asp:Label ID="lblGradCandidacy" runat="server" CssClass="facetLabel" Text="Graduation Candidacy:" />
            <asp:DropDownList ID="ddlGradCandidacy" runat="server" CssClass="facetOptions">
                <asp:ListItem Text="Any" Value="" />
                <asp:ListItem Text="Plan to graduate" Value="Y" />
                <asp:ListItem Text="Do not plan to graduate" Value="N" />
            </asp:DropDownList>
        </div>
    </asp:Panel>
</fieldset>

<asp:Table ID="tblOffices" CssClass="tblFacetedSearch" runat="server" CellPadding="0" CellSpacing="0">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell ColumnSpan="8">Office Tasks</asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableHeaderRow>
        <asp:TableHeaderCell>&nbsp;</asp:TableHeaderCell>
        <asp:TableHeaderCell>Yes</asp:TableHeaderCell>
        <asp:TableHeaderCell>No</asp:TableHeaderCell>
        <asp:TableHeaderCell>Pending</asp:TableHeaderCell>
        <asp:TableHeaderCell>Waived</asp:TableHeaderCell>
        <asp:TableHeaderCell>Complete (Y/W)</asp:TableHeaderCell>
        <asp:TableHeaderCell>Incomplete (N/P)</asp:TableHeaderCell>
        <asp:TableHeaderCell>Any</asp:TableHeaderCell>
    </asp:TableHeaderRow>
</asp:Table>

<asp:Button ID="btnSearch" runat="server" Text="Search" UseSubmitBehavior="true" OnClick="btnSearch_Click" />
<asp:Button ID="btnExportExcel" runat="server" Text="Export to Excel" OnClick="btnExportExcel_Click" />

<asp:Panel ID="panelResultCount" runat="server" CssClass="resultCount">
    <asp:Literal ID="ltlResultCount" runat="server" /> record(s) match.
</asp:Panel>
<asp:GridView ID="dgResults" runat="server" AutoGenerateColumns="false" CssClass="facetSearchResults">
    <EmptyDataTemplate>
        <asp:Panel ID="panelEmptySearchResults" runat="server" CssClass="pSection">
            <p>No students matched your search</p>
        </asp:Panel>
    </EmptyDataTemplate>
    <Columns>
        <asp:BoundField DataField="HostID" HeaderText="Carthage ID" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="Last Name" HeaderText="Last Name" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="First Name" HeaderText="First Name" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="Email" HeaderText="Email" HeaderStyle-CssClass="hide" ItemStyle-CssClass="hide" />
        
        <asp:BoundField DataField="Admit Year" HeaderText="Admit Year" HeaderStyle-CssClass="hide" ItemStyle-CssClass="hide" />
        <asp:BoundField DataField="Admit Term" HeaderText="Admit Term" HeaderStyle-CssClass="hide" ItemStyle-CssClass="hide" />
        <asp:BoundField DataField="ClassCode" HeaderText="Classification Code" HeaderStyle-CssClass="hide" ItemStyle-CssClass="hide" />
        <asp:BoundField DataField="AcademicStanding" HeaderText="Academic Standing" HeaderStyle-CssClass="hide" ItemStyle-CssClass="hide" />

        <asp:BoundField DataField="Payment Options Form" HeaderText="Payment Options" />
        <asp:BoundField DataField="Financial Clearance" HeaderText="Financial Clearance" />
        <asp:BoundField DataField="Room And Board" HeaderText="Room &amp; Board" />
        <asp:BoundField DataField="Entrance Counseling" HeaderText="Entrance Counseling" />
        <asp:BoundField DataField="Stafford Loan" HeaderText="Stafford Loan" />
        <asp:BoundField DataField="No Missing Documents" HeaderText="No Missing Docs" />
        <asp:BoundField DataField="Verification Worksheet" HeaderText="Verification Worksheet" />
        <asp:BoundField DataField="Medical Forms" HeaderText="Medical Forms" />
        <asp:BoundField DataField="Ferpa Release" HeaderText="Directory FERPA" />
        <asp:BoundField DataField="Verify Address" HeaderText="Verify Address" />
        <asp:BoundField DataField="Verify Majors" HeaderText="Verify Major" />
        <asp:BoundField DataField="Distribute Schedule" HeaderText="Schedule" />
        <asp:BoundField DataField="Community Code" HeaderText="Community Code" />
        <asp:BoundField DataField="Parking Permit" HeaderText="Parking Permit" />
        <%---
        <asp:BoundField DataField="id" HeaderText="Carthage ID" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="lastname" HeaderText="Last Name" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="firstname" HeaderText="First Name" ItemStyle-CssClass="basicInfo" />
        <asp:BoundField DataField="payment_options_form" HeaderText="Payment Options" />
        <asp:BoundField DataField="room_and_board" HeaderText="Room &amp; Board" />
        <asp:BoundField DataField="entrance_counseling" HeaderText="Entrance Counseling" />
        <asp:BoundField DataField="perkins_loan_entrance" HeaderText="Perkins Entrance" />
        <asp:BoundField DataField="perkins_loan_master" HeaderText="Perkins MPN" />
        <asp:BoundField DataField="stafford_loan" HeaderText="Stafford Loan" />
        <asp:BoundField DataField="no_missing_documents" HeaderText="No Missing Docs" />
        <asp:BoundField DataField="verification_worksheet" HeaderText="Verification Worksheet" />
        <asp:BoundField DataField="medical_forms" HeaderText="Medical Forms" />
        <asp:BoundField DataField="ferpa_release" HeaderText="Directory FERPA" />
        <asp:BoundField DataField="verify_address" HeaderText="Verify Address" />
        <asp:BoundField DataField="verify_majors" HeaderText="Verify Major" />
        <asp:BoundField DataField="distribute_schedule" HeaderText="Schedule" />
        <asp:BoundField DataField="confirm_graduate_status" HeaderText="Grad Candidacy" />
        <asp:BoundField DataField="community_code" HeaderText="Community Code" />
        <asp:BoundField DataField="housing_survey" HeaderText="Housing Survey" />
        <asp:BoundField DataField="parking_permit" HeaderText="Parking Permit" />
        ---%>
    </Columns>
</asp:GridView>