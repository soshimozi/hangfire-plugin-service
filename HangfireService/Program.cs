﻿using System;
using System.ServiceProcess;

namespace HangfireService
{

    static class Program
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string [] argv)
        {
            logger.Info("Test");

            if (argv.Length > 0 && argv[0] == "/debug")
            {
                var service = new HangfirePluginService();
                service.DebugRun();
                Console.WriteLine("Press any key to stop program");
                Console.Read();
                service.DebugStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new HangfirePluginService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
