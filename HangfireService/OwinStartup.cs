using System.Configuration;
using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HangfireService.OwinStartup))]
namespace HangfireService
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString);
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();
        }
    }
}
