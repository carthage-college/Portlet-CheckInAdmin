<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Detail_Student.ascx.cs" Inherits="Portlet.CheckInAdmin.Detail_Student" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<script type="text/javascript">
    $(function () {
        $.each($('.taskTable tbody tr').not(':first'), function (obj, index) {
            var taskStatus = $(this).find('td:eq(2) input[disabled="disabled"]').val();
            $(this).find('td:first').addClass(taskStatus == 'Yes' || taskStatus == 'Waived' ? 'complete' : 'incomplete');
        });

        /* Admissions requested change to highlight each completed row, rather than grouped by office */
        /*
        //Get all rows of the table
        var $tableRows = $('.taskTable tbody tr').not(':first');
        var offices = [];
        //Loop through rows to get the names of every office
        $tableRows.each(function (index) {
            offices.push($(this).find('td:first').text());
        });
        //$.unique() removes duplicate entries from the array
        $.each($.unique(offices), function (officeIndex, currentOffice) {
            var officeComplete = true;
            //Loop through every row of the table
            $tableRows.each(function (rowIndex) {
                //If the row is associated with the current office...
                if ($(this).find('td:first').text() == currentOffice) {
                    //Get the current task status
                    var taskStatus = $(this).find('td:eq(2) input[disabled="disabled"]').val();
                    //If the task is not completed, the entire office is flagged as incomplete
                    if (taskStatus == 'No' || taskStatus == 'Pending') {
                        officeComplete = false;
                    }
                }
            });

            //Determine css class to use
            var rowClass = officeComplete ? 'complete' : 'incomplete';
            $tableRows.each(function (rowIndex) {
                if ($(this).find('td:first').text() == currentOffice) {
                    //Attach class to cell if the office matches
                    $(this).find('td:first').addClass(rowClass);
                }
            });
        });
        */
    });
</script>

<common:SubHeader ID="shDetail" runat="server" Text="Student Detail View" />
<!---
<h2>Student Detail View</h2>
<div class="pSection">
    <p><asp:Literal ID="ltlStudentName" runat="server" /></p>
    <p>Student ID: <asp:Literal ID="ltlStudentID" runat="server" /></p>
</div>
--->

<div class="pSection">
    <asp:Button ID="btnBackToSearch1" runat="server" Text="Return to Search" UseSubmitBehavior="true" OnClick="btnBackToSearch_Click" />
</div>

<asp:DataGrid ID="dgTasks" runat="server" AutoGenerateColumns="false" CssClass="taskTable" Width="100%" OnItemDataBound="dgTasks_ItemDataBound" DataKeyField="TaskID">
    <HeaderStyle Font-Bold="true" HorizontalAlign="Center" BackColor="#AAAAAA" />
    <Columns>
        <asp:BoundColumn DataField="OfficeName" HeaderText="Office" />
        <asp:BoundColumn DataField="TaskName" HeaderText="Task" />
        <%--- <asp:BoundColumn DataField="TaskStatus" HeaderText="Status" ItemStyle-HorizontalAlign="Center" /> ---%>
        <%---
        <asp:TemplateColumn>
            <ItemTemplate>
                <fieldset data-role="controlgroup" data-type="horizontal">
                    <asp:RadioButton ID="radioStatusY" runat="server" GroupName="radioStatus" Text="Yes" OnCheckedChanged="radioStatusY_CheckedChanged" />
                    <asp:RadioButton ID="radioStatusN" runat="server" GroupName="radioStatus" Text="No" OnCheckedChanged="radioStatusN_CheckedChanged" />
                    <asp:RadioButton ID="radioStatusP" runat="server" GroupName="radioStatus" Text="Pending" OnCheckedChanged="radioStatusP_CheckedChanged" />
                    <asp:RadioButton ID="radioStatusW" runat="server" GroupName="radioStatus" Text="Waived" OnCheckedChanged="radioStatusW_CheckedChanged" />
                </fieldset>
            </ItemTemplate>
        </asp:TemplateColumn>
        ---%>
        <asp:TemplateColumn HeaderText="Status">
            <ItemTemplate>
                <asp:Button ID="btnStatusY" runat="server" Text="Yes" OnClick="btnStatusY_Click" />
                <asp:Button ID="btnStatusN" runat="server" Text="No" OnClick="btnStatusN_Click" />
                <asp:Button ID="btnStatusP" runat="server" Text="Pending" OnClick="btnStatusP_Click" />
                <asp:Button ID="btnStatusW" runat="server" Text="Waived" OnClick="btnStatusW_Click" />
            </ItemTemplate>
        </asp:TemplateColumn>
        <asp:BoundColumn DataField="CompletedBySystem" HeaderText="System" />
        <asp:BoundColumn DataField="StatusDate" HeaderText="Date" DataFormatString="{0:MMMM d, yyyy}" />
        <asp:BoundColumn DataField="StatusByID" HeaderText="Changed By" />
        <asp:BoundColumn DataField="StatusReason" HeaderText="Reason for Change" />
    </Columns>
</asp:DataGrid>

<div class="pSection">
    <asp:Button ID="btnBackToSearch2" runat="server" Text="Return to Search" UseSubmitBehavior="true" OnClick="btnBackToSearch_Click" />
</div>