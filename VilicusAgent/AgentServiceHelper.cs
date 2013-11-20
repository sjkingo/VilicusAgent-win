using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace VilicusAgent
{
    static class AgentServiceHelper
    {
        public static string serviceName = "VilicusAgent";

        public static bool IsInstalled()
        {
            using (ServiceController controller = new ServiceController(serviceName))
            {
                try
                {
                    ServiceControllerStatus status = controller.Status;
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        private static AssemblyInstaller GetInstaller()
        {
            AssemblyInstaller installer = new AssemblyInstaller(typeof(ProjectInstaller).Assembly, null);
            installer.UseNewContext = true;
            return installer;
        }

        public static void InstallService()
        {
            if (IsInstalled())
            {
                Console.Error.WriteLine("The " + serviceName + " service is already installed");
                return;
            }

            using (AssemblyInstaller installer = GetInstaller())
            {
                IDictionary state = new Hashtable();
                installer.Install(state);
                installer.Commit(state);
            }
        }

        public static void UninstallService()
        {
            if (!IsInstalled())
            {
                Console.Error.WriteLine("The " + serviceName + " service is not installed");
                return;
            }

            using (AssemblyInstaller installer = GetInstaller())
            {
                IDictionary state = new Hashtable();
                installer.Uninstall(state);
            }
        }
    }
}
