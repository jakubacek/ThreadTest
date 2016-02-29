<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebTestHangfire._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">  
    <asp:Button runat="server" ID="btnStartClient" OnClick="btnStartClient_OnClick" Text="Start back task"/>
</asp:Content>
