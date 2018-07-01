using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Contracts
{
    public interface IPluginHandler
    {
        void Handle(params object[] Parameters);
    }
}
