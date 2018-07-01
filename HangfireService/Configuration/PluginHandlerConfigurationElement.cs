using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Configuration
{
    public class PluginHandlerConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("plugin-name", IsKey = true, IsRequired = true)]
        public string PluginName
        {
            get { return (string)base["plugin-name"]; }
            set { base["plugin-name"] = value; }
        }

        [ConfigurationProperty("cron-expression", IsRequired = true)]
        public string CronExpression
        {
            get { return (string)base["cron-expression"]; }
            set { base["cron-expression"] = value; }
        }
    }
}
