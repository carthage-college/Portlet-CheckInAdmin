<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="Portlet.CheckInAdmin.Dashboard" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>
<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<common:Subheader ID="shDashboard" runat="server" Text="Check-In Administrative Dashboard" />

<asp:Panel ID="panelNavigation" runat="server" CssClass="clear">
    <asp:LinkButton ID="aFacetSearch" runat="server" Text="Search by Student Progress" OnClick="aFacetSearch_Click" /> |
    <asp:LinkButton ID="aNameSearch" runat="server" Text="Search by Student Name/ID" OnClick="aNameSearch_Click" /> |
    <asp:LinkButton ID="aRoot" runat="server" Text="Site Admin Tools (Admin only)" Visible="false" OnClick="aRoot_Click" />
    <common:ContentBox ID="contentDownloads" runat="server" CssClass="dashboardDownload">
        <h4>Reporting/Action Center</h4>

        <asp:Button ID="btnUpdateSMD" runat="server" Text="Update Students to Check In" OnClick="btnUpdateSMD_Click" />
        <asp:Button ID="btnUpdateRegStat" runat="server" Text="Update reg_stat" OnClick="btnUpdateRegStat_Click" />

        <div class="pSection">
            <h4>Download Excel File</h4>
            <asp:Button ID="btnIncomplete" runat="server" Text="Students with tasks remaining" OnClick="btnIncomplete_Click" />
            <!--- Grid for export of "Students with tasks remaining" --->
            <asp:GridView ID="gvIncomplete" runat="server" Visible="false" AutoGenerateColumns="false" GridLines="Both">
                <Columns>
                    <asp:BoundField DataField="HostID" HeaderText="Carthage ID" />
                    <asp:BoundField DataField="lastname" HeaderText="Last Name" />
                    <asp:BoundField DataField="firstname" HeaderText="First Name" />
                    <asp:BoundField DataField="CompletedTasks" HeaderText="Completed Tasks" />
                    <asp:BoundField DataField="TotalTasks" HeaderText="Total Tasks" />
                    <asp:BoundField DataField="PercentComplete" HeaderText="Percent Complete" />
                    <asp:BoundField DataField="email" HeaderText="Email" />
                    <asp:BoundField DataField="TaskList" HeaderText="Incomplete Tasks" />
                </Columns>
            </asp:GridView>

            <asp:Button ID="btnNotStarted" runat="server" Text="Students who have not started" OnClick="btnNotStarted_Click" />
            <asp:GridView ID="gvNotStarted" runat="server" Visible="false" AutoGenerateColumns="false" GridLines="Both">
                <Columns>
                    <asp:BoundField DataField="HostID" HeaderText="Carthage ID" />
                    <asp:BoundField DataField="lastname" HeaderText="Last Name" />
                    <asp:BoundField DataField="firstname" HeaderText="First Name" />
                    <asp:BoundField DataField="email" HeaderText="Email" />
                </Columns>
            </asp:GridView>
        </div>
    </common:ContentBox>
</asp:Panel>

<asp:Panel ID="panelCheckInSummary" runat="server">
    <common:Subheader ID="shCheckInSummary" runat="server" Text="Check-In Summary" />
    <asp:Chart ID="chartCheckInSummary" runat="server" Width="900">
        <Legends>
            <asp:Legend IsEquallySpacedItems="true" IsTextAutoFit="true" />
        </Legends>
        <Series>
            <asp:Series Name="Not Logged In: Student has not accessed check-in" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="NotLoggedIn" />
            <asp:Series Name="No Tasks Completed: There are 0 check-in tasks marked 'Yes'" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="NothingCompleted" />
            <asp:Series Name="No Tasks Completed/Waived: There are 0 check-in tasks marked 'Yes' or 'Waived'" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="NothingCompletedWaived" />
            <asp:Series Name="One Task Remaining: Student is missing only one task" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="OneTaskRemaining" />
            <asp:Series Name="Finished Check-In: Student has finished all check-in tasks" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Finished" />
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="caCheckInSummary" Area3DStyle-Enable3D="true" />
        </ChartAreas>
    </asp:Chart>
</asp:Panel>

<asp:Panel ID="panelStudentProgress" runat="server" CssClass="chart">
    <common:Subheader ID="shStudentProgress" runat="server" Text="Student Progress" />
    <!--- Bar chart breaking down Complete, Started, Not Started --->
    <asp:Chart ID="chartStudentProgress" runat="server" Width="700" Height="500">
<%--        <Legends>
            <asp:Legend IsEquallySpacedItems="true" IsTextAutoFit="true" />
        </Legends>--%>
        <Titles>
            <asp:Title Text="Chart Title: Student Progress" />
        </Titles>
        <Series>
            <asp:Series Name="Tasks Completed" ChartType="Column" IsValueShownAsLabel="true" YValueMembers="StudentCount"></asp:Series>
<%--            <asp:Series Name="Not Started" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="NotStarted" />
            <asp:Series Name="Started" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Started" />
            <asp:Series Name="One More" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Missing1" />
            <asp:Series Name="Complete" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Complete" />--%>
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="caStudentProgress" Area3DStyle-Enable3D="true">
                <AxisX Title="Tasks Completed" Interval="1" Minimum="0" />
                <AxisY Title="Number of Students" />
            </asp:ChartArea>
        </ChartAreas>
    </asp:Chart>
</asp:Panel>

<asp:Panel ID="panelStudentActivity" runat="server" CssClass="chart">
    <common:Subheader ID="shStudentActivity" runat="server" Text="Student Activity" />
    <!--- Line chart breaking down activity by hour groups: 4, 6, 12, 24 --->
    <asp:Chart ID="chartStudentActivity" runat="server" Width="700" Height="500">
        <Legends>
            <asp:Legend IsEquallySpacedItems="true" IsTextAutoFit="true" />
        </Legends>
        <Titles>
            <asp:Title Text="Chart Title: Student Activity" />
        </Titles>
        <Series>
            <asp:Series Name="Activity" ChartType="Line" IsValueShownAsLabel="true" IsVisibleInLegend="true" XValueMember="DateLabel" YValueMembers="Completed" />
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="caStudentActivity" Area3DStyle-Enable3D="true">
                <AxisX Title="Day/Time" />
                <AxisY Title="Number of Students" />
            </asp:ChartArea>
        </ChartAreas>
    </asp:Chart>
</asp:Panel>

<div class="clear">&nbsp;</div>