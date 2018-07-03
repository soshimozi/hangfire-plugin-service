using Hangfire;
using Hangfire.Common;
using HangfireService.Configuration;
using HangfireService.Contracts;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace HangfireService
{
    public partial class HangfirePluginService : ServiceBase
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(typeof(HangfirePluginService));

        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private Thread _thread;

        [ImportMany]
        public IEnumerable<Lazy<IPluginHandler, IPluginHandlerMetadata>> _handlers = null;

        private CompositionContainer _container;

        public HangfirePluginService()
        {
            InitializeComponent();
        }

        public void DebugRun()
        {
            OnStart(null);
        }

        public void DebugStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("Starting service");

            _thread = new Thread(ServiceThreadFunc);
            _thread.Name = "Hangfire Plugin Service Thread";
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void ServiceThreadFunc()
        {
            var uri = ConfigurationManager.AppSettings["HangfireUri"];
            var pluginPath = ConfigurationManager.AppSettings["PluginDirectory"];

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var asmName = new AssemblyName(args.Name);
                var plugin = Path.Combine(pluginPath, asmName.Name + ".dll");
                if (File.Exists(plugin))
                    return Assembly.LoadFrom(plugin);
                return null;
            };

            using (var webApp = WebApp.Start<OwinStartup>(uri))
            {

                var manager = new RecurringJobManager();

                InitializeCatalog(pluginPath);
                LoadPlugins(manager);

                logger.Info("Web Application listening.");

                _shutdownEvent.WaitOne();
            }

        }

        private void InitializeCatalog(string pluginPath)
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(typeof(HangfirePluginService).Assembly));

            catalog.Catalogs.Add(new DirectoryCatalog(pluginPath, "*.dll"));
            _container = new CompositionContainer(catalog);

            logger.Info($"Loading plugins from {pluginPath}.");

            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
                throw;
            }
        }

        private void LoadPlugins(RecurringJobManager manager)
        {
            var pluginSection = ConfigurationManager.GetSection("pluginConfiguration") as PluginHandlerConfigurationSection;
            var cronExpressions = pluginSection.Plugins.Cast<PluginHandlerConfigurationElement>().ToDictionary(k => k.PluginName, v => v.CronExpression);

            foreach (var handler in _handlers)
            {
                IPluginHandler handlerInterface = handler.Value;

                var job = Job.FromExpression(() => handler.Value.Handle());

                var cronExpression = string.Empty;
                if (!cronExpressions.TryGetValue(handler.Metadata.Name, out cronExpression))
                {
                    logger.Warn($"Could not find cron expression for handler named {handler.Metadata.Name}.  Handler will not be loaded.");
                    continue;
                }

                logger.Info($"Loading handler {handler.Metadata.Name} using expression {cronExpression}");
                manager.AddOrUpdate(handler.Metadata.Name, job, cronExpression);
            }
        }

        protected override void OnStop()
        {
            _shutdownEvent.Set();

            if (!_thread.Join(10000))
            {
                _thread.Abort();
            }
        }


    }
}
