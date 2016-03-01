<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebTestHangfire._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">  
    <asp:Button runat="server" ID="btbStartFeed" OnClick="btbStartFeed_OnClick" Text="Start get news feeds"/>
    <asp:Button runat="server" ID="btnStartCommunication" OnClick="btnStartCommunication_OnClick" Text="Start communication with server" OnClientClick="return confirm('Is server running?!');"/>
    <br/>
    <asp:Button runat="server" ID="btnTestCommunication" OnClick="btnTestCommunication_OnClick" Text="Test communication with server" OnClientClick="return confirm('Is server running?!');"/>
</asp:Content>
