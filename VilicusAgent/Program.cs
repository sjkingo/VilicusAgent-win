using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VilicusAgent
{
    static class Program
    {
        private static void RunProgram()
        {
            if (Environment.UserInteractive)
            {
                // Run interactively on a console
                var agent = new Agent(true);
                agent.DoWorkLoop();
            }
            else
            {
                // Started from SCM, fire up a worker service
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { 
                    new AgentService() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "--install":
                        AgentServiceHelper.InstallService();
                        break;
                    case "--uninstall":
                        AgentServiceHelper.UninstallService();
                        break;
                    case "--help":
                    default:
                        Console.WriteLine("VilicusAgent, version " + Agent.VERSION + "\t(C) 2013 SJK Web Industries");
                        Console.WriteLine();
                        Console.WriteLine("Usage: VilicusAgent [--install|--uninstall]");
                        Console.WriteLine("    [no arguments]    Run the agent interactively and output to the console.");
                        Console.WriteLine("    --install         Installs the agent as a Windows service named \"" + AgentServiceHelper.serviceName + "\".");
                        Console.WriteLine("    --uninstall       Uninstalls the Windows service for the agent.");
                        System.Environment.Exit(1);
                        break; // compiler nonsense
                }
            }
            else
            {
                string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly()
                                                         .GetCustomAttributes(typeof(GuidAttribute), false)
                                                         .GetValue(0)).Value.ToString();
                try
                {
                    // Prevent the agent from running more than one instance by locking its GUID
                    using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
                    {
                        if (!mutex.WaitOne(0, false))
                        {
                            throw new UnauthorizedAccessException();
                        }
                        // All good, go.
                        RunProgram();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Another instance of VilicusAgent.exe is running (guid " + appGuid + " is locked.)");
                    System.Environment.Exit(1);
                }
            }
        }
    }
}
