using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Configuration
{
    public class PluginHandlerConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("plugins", IsRequired = true)]
        public PluginHandlerConfigurationCollection Plugins
        {
            get { return (PluginHandlerConfigurationCollection)this["plugins"]; }
            set { this["plugins"] = value; }
        }
    }
}
