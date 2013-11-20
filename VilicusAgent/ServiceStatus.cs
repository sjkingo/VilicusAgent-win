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
        public static Dictionary<string, ServiceControllerStatus> Map = new Dictionary<string, ServiceControllerStatus>()
        {
            { "START_PENDING", ServiceControllerStatus.StartPending },
            { "RUNNING", ServiceControllerStatus.Running },
            { "STOP_PENDING", ServiceControllerStatus.StopPending },
            { "STOPPED", ServiceControllerStatus.Stopped },
            { "PAUSE_PENDING", ServiceControllerStatus.PausePending },
            { "PAUSED", ServiceControllerStatus.Paused },
            { "CONTINUE_PENDING", ServiceControllerStatus.ContinuePending },
            { "NOT_INSTALLED", 0x0 }
        };

        public static string GetFirstKeyFromValue(ServiceControllerStatus value)
        {
            return Map.FirstOrDefault(i => i.Value == value).Key;
        }
    }
}
