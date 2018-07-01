using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Plugins
{
    public interface IPluginHandlerMetadata
    {
        string Name { get; }
    }
}
