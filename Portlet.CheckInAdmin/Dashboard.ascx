<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="Portlet.CheckInAdmin.Dashboard" %>
<%@ Register Assembly="Jenzabar.Common" Namespace="Jenzabar.Common.Web.UI.Controls" TagPrefix="common" %>
<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<common:Subheader ID="shDashboard" runat="server" Text="Check-In Administrative Dashboard" />

<asp:Panel ID="panelNavigation" runat="server" CssClass="clear">
    <asp:LinkButton ID="aFacetSearch" runat="server" Text="Search by Student Progress" OnClick="aFacetSearch_Click" /> |
    <asp:LinkButton ID="aNameSearch" runat="server" Text="Search by Student Name/ID" OnClick="aNameSearch_Click" />
    <common:ContentBox ID="contentDownloads" runat="server" CssClass="dashboardDownload">
        <h4>Download Center</h4>
        <asp:GridView ID="gvIncomplete" runat="server" Visible="false" AutoGenerateColumns="false" GridLines="Both">
            <Columns>
                <asp:BoundField DataField="id" HeaderText="Carthage ID" />
                <asp:BoundField DataField="lastname" HeaderText="Last Name" />
                <asp:BoundField DataField="firstname" HeaderText="First Name" />
                <asp:BoundField DataField="email" HeaderText="Email" />
                <asp:BoundField DataField="phone" HeaderText="Phone" />
            </Columns>
        </asp:GridView>
        <asp:Button ID="btnIncomplete" runat="server" Text="Students with tasks remaining" OnClick="btnIncomplete_Click" />
        <%--<asp:Button ID="btnUpdateProgress" runat="server" Text="Update Progress" OnClick="btnUpdateProgress_Click" />--%>
    </common:ContentBox>
</asp:Panel>

<asp:Panel ID="panelStudentProgress" runat="server" CssClass="chart">
    <common:Subheader ID="shStudentProgress" runat="server" Text="Student Progress" />
    <!--- Bar chart breaking down Complete, Started, Not Started --->
    <asp:Chart ID="chartStudentProgress" runat="server" Width="700" Height="500">
        <Legends>
            <asp:Legend IsEquallySpacedItems="true" IsTextAutoFit="true" />
        </Legends>
        <Titles>
            <asp:Title Text="Chart Title: Student Progress" />
        </Titles>
        <Series>
            <asp:Series Name="Not Started" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="NotStarted" />
            <asp:Series Name="Started" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Started" />
            <asp:Series Name="One More" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Missing1" />
            <asp:Series Name="Complete" ChartType="Bar" IsValueShownAsLabel="true" IsVisibleInLegend="true" YValueMembers="Complete" />
        </Series>
        <ChartAreas>
            <asp:ChartArea Name="caStudentProgress" Area3DStyle-Enable3D="true">
                <AxisX Title="Progress" />
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