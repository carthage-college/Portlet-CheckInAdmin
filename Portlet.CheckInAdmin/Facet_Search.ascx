<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Facet_Search.ascx.cs" Inherits="Portlet.CheckInAdmin.Facet_Search" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

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
    });
</script>

<asp:Button ID="btnIndividualLookup" runat="server" Text="Look up student by ID or Last Name" OnClick="btnIndividualLookup_Click" Visible="false" />

<asp:Panel ID="panelFacetedSearch" runat="server">
    <div class="facet">
        <asp:Label ID="lblStanding" runat="server" CssClass="facetLabel" Text="Standing:" />
        <asp:CheckBoxList ID="cblStanding" runat="server" CssClass="facetOptions" RepeatDirection="Horizontal">
            <asp:ListItem Text="Freshman/Transfer" Value="Y" />
            <asp:ListItem Text="Returning" Value="N" />
        </asp:CheckBoxList>
    </div>
    <div class="facet">
        <asp:Label ID="lblAthlete" runat="server" CssClass="facetLabel" Text="Athlete:" />
        <asp:CheckBoxList ID="cblAthlete" runat="server" CssClass="facetOptions" RepeatDirection="Horizontal">
            <asp:ListItem Text="Yes" Value="Y" />
            <asp:ListItem Text="No" Value="N" />
        </asp:CheckBoxList>
    </div>
    <div class="facet">
        <asp:Label ID="lblResidency" runat="server" CssClass="facetLabel" Text="Residency:" />
        <asp:CheckBoxList ID="cblResidency" runat="server" CssClass="facetOptions" RepeatDirection="Horizontal">
            <asp:ListItem Text="Resident" Value="R" />
            <asp:ListItem Text="Commuter" Value="C" />
            <asp:ListItem Text="Off-Campus" Value="O" />
        </asp:CheckBoxList>
    </div>
    <div class="facet">&nbsp;</div>
</asp:Panel>

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
    </Columns>
</asp:GridView>