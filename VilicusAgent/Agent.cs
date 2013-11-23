using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class Agent
    {
        public const string VERSION = "1.0";

        public API_Agent agent;
        public Log log;

        private bool isInteractive;
        private ApiClient api;
        private List<API_WindowsService> services;
        readonly string appGuid;

        public Agent(bool isInteractive)
        {
            this.isInteractive = isInteractive;
            this.appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            this.log = new Log(isInteractive);

            if (ConfigurationManager.AppSettings["apiURL"] == null)
            {
                throw new ConfigurationErrorsException("Configuration is missing the required option: apiURL");
            }
            if (ConfigurationManager.AppSettings["apiID"] == null)
            {
                throw new ConfigurationErrorsException("Configuration is missing the required option: apiID");
            }

            api = new ApiClient(log);

            LogStartupInfo();
            SetupAgent();
        }

        private void LogStartupInfo()
        {
            string interactivity = "service";
            if (isInteractive)
            {
                interactivity = "interactive";
            }

            log.Info(String.Format("VilicusAgent v{0} starting ({1})", VERSION, interactivity));

            string logging = "Logging to " + log.ToString();
            if (isInteractive) logging += " and stdout";
            log.Info(logging);
        }

        private void SetupAgent()
        {
            int agentId = Convert.ToInt32(ConfigurationManager.AppSettings["apiID"]);

            // Fetch this agent from the manager
            agent = api.GetAgent(agentId);
            if (agent == null)
            {
                throw new ConfigurationErrorsException("The agent ID " + agentId + " does not exist at the manager");
            }

            string last_checkin;
            if (agent.last_checkin == null)
            {
                last_checkin = "never";
            }
            else
            {
                last_checkin = agent.last_checkin.ToString();
            }
            agent.last_checkin = DateTime.Now;

            // Check if we need to update our hostname
            log.Info(String.Format("I am the agent named {0} (id={1}); last checkin was {2}", agent.hostname, agentId, last_checkin));
            var actual_hostname = GetFQDN();
            if (agent.hostname != actual_hostname)
            {
                log.Info(String.Format("My configured hostname ({0}) differs from my actual ({1}), fixing", agent.hostname, actual_hostname));
                agent.hostname = actual_hostname;
            }

            // Check if we need to update the version
            if (agent.version != VERSION)
            {
                log.Info(String.Format("Updating version information on manager from {0} to {1}", agent.version, VERSION));
                agent.version = VERSION;
            }

            // Check if we need to update the GUID
            
            if (agent.guid != appGuid)
            {
                log.Info(String.Format("Updating GUID on manager from {0} to {1}", agent.guid, appGuid));
                agent.guid = appGuid;
            }

            api.SendAgentUpdate(agent);
        }

        private string GetFQDN()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return string.IsNullOrWhiteSpace(ipProperties.DomainName) ? ipProperties.HostName
                    : string.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
        }

        private void FetchServicesFromManager()
        {
            var r = api.GetServices(agent);
            if (r == null)
            {
                log.Error("Failed to fetch list of services from manager.");
                return;
            }

            services = r.objects;
            if (services.Count == 0)
            {
                log.Warn("There are no configured services for this agent.");
                return;
            }

            log.Debug("Configured services:");
            foreach (var service in services)
            {
                log.Debug(String.Format("  {0}: {1} (expecting state {2})", service.id, service.service_name, service.expected_status));
            }
        }

        private void SendWindowsServerStatus(API_WindowsService service, ServiceController sc, string action_taken)
        {
            var l = new API_WindowsServiceLog();
            l.service = service.resource_uri;
            l.timestamp = DateTime.Now;
            l.expected_status = service.expected_status;
            if (sc != null)
            {
                l.actual_status = ServiceState.GetFirstKeyFromValue(sc.Status);
            }
            else
            {
                l.actual_status = "NOT_INSTALLED";
            }
            l.action_taken = action_taken;
            api.SendWindowsServiceLog(l);
        }

        private void UpdatePerfLog()
        {
            var p = new API_PerformanceLogEntry();
            p.agent = agent.resource_uri;
            p.timestamp = DateTime.Now;

            var cpuload = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuload.NextValue(); // throwaway 0
            Thread.Sleep(500); // 500 ms so the next value is accurate
            p.cpu_usage = Convert.ToInt32(cpuload.NextValue());
            Console.WriteLine("CPU " + p.cpu_usage + "%");

            api.SendPerfLog(p);
        }

        public void DoWork()
        {
            // Log performance data first.
            // TODO: this needs to be in its own thread
            UpdatePerfLog();

            // Now update Windows Services.
            FetchServicesFromManager();

            // Check services.
            foreach (var service in services)
            {
                ServiceController sc = new ServiceController(service.service_name);
                try
                {
                    var exp = ServiceState.Map[service.expected_status];
                    if (exp != sc.Status)
                    {
                        log.Warn(String.Format("Service {0} is in state {1} when it was expected to be in state {2}.",
                            service.service_name, ServiceState.GetFirstKeyFromValue(sc.Status), service.expected_status));
                    }
                    SendWindowsServerStatus(service, sc, "TODO");
                }
                catch (InvalidOperationException)
                {
                    if (service.expected_status != "NOT_INSTALLED")
                    {
                        log.Warn(String.Format("Service {0} is in state NOT_INSTALLED when it was expected to be in state {1}.",
                            service.service_name, service.expected_status));
                    }
                    SendWindowsServerStatus(service, null, "TODO");
                }
            }
        }

        public void DoWorkLoop()
        {
            while (true)
            {
                DoWork();
                Thread.Sleep(agent.check_interval_ms);
            }
        }
    }
}
