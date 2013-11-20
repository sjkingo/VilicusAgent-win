using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    static class ServiceStatus
    {
        public static ServiceControllerStatus Resolve(string name)
        {
            switch (name)
            {
                case "START_PENDING": return ServiceControllerStatus.StartPending;
                case "RUNNING": return ServiceControllerStatus.Running;
                case "STOP_PENDING": return ServiceControllerStatus.StopPending;
                case "STOPPED": return ServiceControllerStatus.Stopped;
                case "PAUSE_PENDING": return ServiceControllerStatus.PausePending;
                case "PAUSED": return ServiceControllerStatus.Paused;
                case "CONTINUE_PENDING": return ServiceControllerStatus.ContinuePending;
            }
            return 0x0; // yuk
        }
    }
}
