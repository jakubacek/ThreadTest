<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebTestLongTask.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ahoj</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>        

      Worker type:
          <asp:RadioButtonList runat="server" ID="rbtWorkerType">
            <asp:ListItem Text="Queue threads" Value="0"></asp:ListItem>
            <asp:ListItem Text="Queue tasks" Value="1"></asp:ListItem>
            <asp:ListItem Text="Start tasks" Value="2"></asp:ListItem>
            <asp:ListItem Text="Start thread" Value="3"></asp:ListItem>
            <asp:ListItem Text="Start threadpool" Value="4"></asp:ListItem>
        </asp:RadioButtonList>
        
       <br/> Client type:
        <asp:RadioButtonList runat="server" ID="rbtClientType">
            <asp:ListItem Text="Client" Value="0"></asp:ListItem>
            <asp:ListItem Text="Client async" Value="1"></asp:ListItem>            
        </asp:RadioButtonList>
       Worker amount: <asp:TextBox runat="server" Text="50" ID="txtWorkerAmount"></asp:TextBox>
        <br/>
       Sleep time ms: <asp:TextBox runat="server" Text="50" ID="txtSleepTime"></asp:TextBox>
        <br/>
        <asp:Button runat="server" OnClick="StartTask" Text="START"/>
        <asp:Button runat="server" OnClick="CancelTask" Text="STOP"/>
    </div>
    </form>
</body>
</html>
