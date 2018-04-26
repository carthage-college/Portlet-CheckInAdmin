<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SiteAdminTools.ascx.cs" Inherits="Portlet.CheckInAdmin.SiteAdminTools" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>

<%--<script src="https://code.jquery.com/jquery-1.12.4.js"></script>
<script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>
<link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />--%>
<script type="text/javascript">
    $(function () {
        $('#<%= this.panelInitializationSteps.ClientID %>').accordion({
            heightStyle: 'content'
        });
    });
</script>
<style type="text/css">
    .row {margin:5px 0px;clear:both;}
    li.withBullets {margin-left:45px;line-height:1.3em;}
</style>


<asp:GridView ID="gvConfigSettings" runat="server" AutoGenerateColumns="false" DataKeyNames="ID" OnRowCancelingEdit="gvConfigSettings_RowCancelingEdit"
    OnRowEditing="gvConfigSettings_RowEditing" OnRowUpdating="gvConfigSettings_RowUpdating" ShowFooter="false" EmptyDataText="No configuration values">
    <Columns>
        <asp:TemplateField HeaderText="Configuration Key" HeaderStyle-HorizontalAlign="Center">
            <ItemTemplate>
                <asp:Label ID="lblKey" runat="server" Text='<%# Eval("ConfigKey") %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtKey" runat="server" Text='<%# Eval("ConfigKey") %>' ReadOnly="true" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Configuration Value" HeaderStyle-HorizontalAlign="Center">
            <ItemTemplate>
                <asp:Label ID="lblValue" runat="server" Text='<%# Eval("ConfigValue") %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtValue" runat="server" Text='<%# Eval("ConfigValue") %>' />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Default Value" HeaderStyle-HorizontalAlign="Center">
            <ItemTemplate>
                <asp:Label ID="lblDefault" runat="server" Text='<%# Eval("ConfigDefaultValue") %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:CommandField ButtonType="Link" ShowEditButton="true" ShowDeleteButton="false" />
    </Columns>
</asp:GridView>

<asp:Panel ID="panelInitializationSteps" runat="server">
    <h3>OfficeTaskSession Initialization</h3>
    <asp:Panel ID="panelTaskSessionInitialization" runat="server">
        <p><asp:Label ID="lblOTSCount" runat="server" /> task(s) initialized for <asp:Label ID="lblSession1" runat="server" /> <asp:Label ID="lblYear1" runat="server" />.
            <asp:Label ID="lblOTCount" runat="server" Text="0" /> additional task(s) available.</p>
        <common:ContentBox ID="contentTaskSessionInit" runat="server">
            <p>Do not initialize the following tasks for spring.</p>
            <ul class="withBullets">
                <li class="withBullets">Financial Aid - Perkins Loan Entrance</li>
                <li class="withBullets">Financial Aid - Perkins Loan Master</li>
                <li class="withBullets">Registrar - Confirm Grad</li>
                <li class="withBullets">Student Affairs - Housing Survey</li>
            </ul>
        </common:ContentBox>

        <div class="row">
            <label>Rollover Year/Term</label>
            <asp:DropDownList ID="ddlInitRolloverYear" runat="server">
                <asp:ListItem Text="" Value=""></asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlInitRolloverSession" runat="server">
                <asp:ListItem Text="" Value=""></asp:ListItem>
                <asp:ListItem Text="RA" Value="RA"></asp:ListItem>
                <asp:ListItem Text="RC" Value="RC"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="row">
            <asp:CheckBoxList ID="cblOfficeTaskSession" runat="server" DataTextField="OfficeTaskLabel" DataValueField="TaskID"></asp:CheckBoxList>
        </div>
        <div class="row">
            <asp:Button ID="btnAddOTS" runat="server" Text="Add Office Tasks for session" UseSubmitBehavior="false" OnClick="btnAddOTS_Click" />
        </div>
    </asp:Panel>

    <h3>OfficeTaskSession Rollover Settings</h3>
    <asp:Panel ID="panelTaskSessionRollover" runat="server">
        <common:ContentBox ID="contentRollover" runat="server">
            <div style="float:left;margin-right:35px">
                <p>Should rollover from Fall for Spring:</p>
                <ul class="withBullets">
                    <li class="withBullets">Student Accounts - Payment Option</li>
                    <li class="withBullets">Student Accounts - Room and Board</li>
                    <li class="withBullets">Campus Nurse - Medical Forms</li>
                    <li class="withBullets">Registrar - FERPA Release</li>
                    <li class="withBullets">Student Affairs - Community Code</li>
                    <li class="withBullets">Security - Parking Permit</li>
                </ul>
            </div>
            <div style="float:left;">
            <p>Should not rollover from Fall for Spring:</p>
                <ul class="withBullets">
                    <li class="withBullets">Student Accounts - Financial Clearance</li>
                    <li class="withBullets">Financial Aid - Stafford Loan</li>
                    <li class="withBullets">Financial Aid - Verification Worksheet</li>
                    <li class="withBullets">Financial Aid - Entrance Counseling</li>
                    <li class="withBullets">Financial Aid - Missing Documents</li>
                    <li class="withBullets">Registrar - Verify Address</li>
                    <li class="withBullets">Registrar - Distribute Schedule</li>
                    <li class="withBullets">Registrar - Verify Major/Minor</li>
                </ul>
            </div>
            <div class="row">&nbsp;</div>
        </common:ContentBox>

        <div class="row">
            <label>Pull status from session:</label>
            <asp:DropDownList ID="ddlRolloverTaskYear" runat="server">
                <asp:ListItem Text="" Value=""></asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlRolloverTaskSession" runat="server">
                <asp:ListItem Text="" Value=""></asp:ListItem>
                <asp:ListItem Text="RA" Value="RA"></asp:ListItem>
                <asp:ListItem Text="RC" Value="RC"></asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="row">
            <asp:CheckBoxList ID="cblRolloverTask" runat="server" DataTextField="OfficeTaskLabel" DataValueField="OfficeTaskSessionID"></asp:CheckBoxList>
        </div>
        <div class="row">
            <asp:Button ID="btnAddRollover" runat="server" Text="Update Rollover Settings" UseSubmitBehavior="false" OnClick="btnAddRollover_Click" />
        </div>
    </asp:Panel>

    <h3>Load Student Meta Data</h3>
    <asp:Panel ID="panelStudentMetaData" runat="server">
        <p>Identified <asp:Label ID="lblSMDCount" runat="server" /> record(s) in CX to bring over to CI_StudentMetaData.</p>
        <div class="row">
            <asp:Button ID="btnLoadSMD" runat="server" Text="Import Students from CX" UseSubmitBehavior="false" OnClick="btnLoadSMD_Click" />
        </div>
    </asp:Panel>

    <h3>Initialize Student Progress</h3>
    <asp:Panel ID="panelStudentProgress" runat="server">
        <div class="row">
            <asp:Button ID="btnInitStudentProgress" runat="server" Text="Initialize student progress" UseSubmitBehavior="false" OnClick="btnInitStudentProgress_Click" />
        </div>
    </asp:Panel>

    <h3>Process Rollover Tasks</h3>
    <asp:Panel ID="panelProcessRollover" runat="server">
        <div class="row">
            <asp:Button ID="btnProcessRollover" runat="server" Text="Process rollover tasks" UseSubmitBehavior="false" OnClick="btnProcessRollover_Click" />
        </div>
    </asp:Panel>
</asp:Panel>