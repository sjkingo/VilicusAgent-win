using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    static class Program
    {

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
                    ServicesToRun = new ServiceBase[] 
                    { 
                        new AgentService() 
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
        }
    }
}
