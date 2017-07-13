<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Detail_Student.ascx.cs" Inherits="Portlet.CheckInAdmin.Detail_Student" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<common:SubHeader ID="shDetail" runat="server" Text="Student Detail View" />
Student ID: <asp:Literal ID="ltlStudentID" runat="server" />

<div class="pSection">
    <asp:Button ID="btnBackToSearch1" runat="server" Text="Return to Search" UseSubmitBehavior="false" OnClick="btnBackToSearch_Click" />
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
        <asp:BoundColumn DataField="CompletedOn" HeaderText="Date" DataFormatString="{0:MMMM d, yyyy}" />
        <asp:BoundColumn DataField="StatusReason" HeaderText="Reason for Change" />
    </Columns>
</asp:DataGrid>

<div class="pSection">
    <asp:Button ID="btnBackToSearch2" runat="server" Text="Return to Search" UseSubmitBehavior="false" OnClick="btnBackToSearch_Click" />
</div>