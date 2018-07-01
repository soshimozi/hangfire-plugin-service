using System;
using System.Configuration;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Owin;
using Hangfire.MemoryStorage;
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
