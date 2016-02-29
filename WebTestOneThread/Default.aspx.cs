using System;

namespace WebTestOneThread
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            h1Counter.InnerText = ((Int32?)Application["Counter"]).ToString();
        }
    }
}