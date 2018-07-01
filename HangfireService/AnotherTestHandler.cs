using HangfireService.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService
{
    [Export(typeof(IPluginHandler))]
    [ExportMetadata("Name", "MyHandler")]
    public class AnotherTestHandler : IPluginHandler
    {
        public void Handle(params object [] Parameters)
        {
            Console.WriteLine("Handled from the plugin!");
        }
    }
}
