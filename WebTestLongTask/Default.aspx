<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebTestLongTask.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ahoj</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button runat="server" OnClick="CancelTask" Text="STOP"/>
    </div>
    </form>
</body>
</html>
