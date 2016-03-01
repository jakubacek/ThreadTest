using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Hangfire;
using WebGrease.Css.Extensions;
using Di = System.Diagnostics;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using WebTestHangFireTasks;
using Hangfire;

namespace WebTestHangfire
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btbStartFeed_OnClick(object sender, EventArgs e)
        {
            BackgroundJob.Enqueue(() => TaskRepo.GetFeedsAndWriteTrace());
        }


        protected void btnStartCommunication_OnClick(object sender, EventArgs e)
        {
            var feed = TaskRepo.GetFeeds();
            BackgroundJob.Enqueue(() => TaskRepo.SendMessagesToServer(feed));
        
        }

        protected void btnTestCommunication_OnClick(object sender, EventArgs e)
        {
            TaskRepo.SendMessagesToServer(new [] {"Test komunikace 1", "Test komunikace 2", "Test komunikace 3" });
        }
    }
}