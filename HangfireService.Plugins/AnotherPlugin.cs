using HangfireService.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Plugins
{
    [Export(typeof(IPluginHandler))]
    [ExportMetadata("Name", "AnotherPlugin")]
    public class AnotherPlugin : IPluginHandler
    {
        public async Task Handle()
        {
            Console.WriteLine("Inside Another Plugin");
            
        }
    }
}
