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
    [ExportMetadata("Name", "S3UpdatePlugin")]
    public class S3UpdatePlugin : IPluginHandler
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(typeof(S3UpdatePlugin));

        public void Handle(params object[] Parameters)
        {
            logger.Info("S3UpdatePlugin.Handle");
        }
    }
}
