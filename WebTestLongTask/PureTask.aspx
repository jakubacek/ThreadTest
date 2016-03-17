<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PureTask.aspx.cs" Inherits="WebTestLongTask.PureTask" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox runat="server" ID="txtIpdAddress" Text="127.0.0.1"></asp:TextBox> <br/>
        <asp:TextBox runat="server" ID="txtDelay" Text="20"></asp:TextBox> <br/>
        <asp:Button runat="server" ID="btnStart" Text="Start workers" OnClick="btnStart_OnClick" OnClientClick="return confirm('Je server zapnuty?')"/>
        <br/>
        <asp:Button runat="server" ID="btnStop" Text="Stop workes" OnClick="btnStop_OnClick"/>
        </div>
    </form>
</body>
</html>
