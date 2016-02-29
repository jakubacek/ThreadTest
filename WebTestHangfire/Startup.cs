using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebTestHangfire.Startup))]

namespace WebTestHangfire
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("MainDb");

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}