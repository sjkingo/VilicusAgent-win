using System;
using System.Collections.Generic;
using System.Configuration;
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
        public static string VERSION = "1.0";

        public Log log;
        public API_Agent agentConfig;

        private bool _isInteractive;
        private API _api;
        private List<API_WindowsService> _services;
        readonly string _appGuid;

        delegate void FailureDelegate(string s);

        public Agent(bool isInteractive)
        {
            this._isInteractive = isInteractive;
            this._appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
            SetupLogFile();
            ValidateConfig();
            SetupAPI();
        }

        private void SetupLogFile()
        {
            string filename = ConfigurationManager.AppSettings["logFilename"];
            if (filename == null)
            {
                filename = "agent.log";
            }

            bool debugFlag;
            if (ConfigurationManager.AppSettings["logDebug"] != null)
            {
                debugFlag = Convert.ToBoolean(ConfigurationManager.AppSettings["logDebug"]);
            }
            else
            {
                debugFlag = false;
            }

            log = new Log(filename, _isInteractive, debugFlag);
        }

        private void ValidateConfig()
        {
            FailureDelegate Fail = (x) =>
            {
                log.Error("Configuration is missing a required option: " + x);
                System.Environment.Exit(2);
            };

            // Required options
            if (ConfigurationManager.AppSettings["apiURL"] == null) Fail("apiURL");
            if (ConfigurationManager.AppSettings["apiID"] == null) Fail("apiID");
        }

        private void SetupAPI()
        {
            _api = new API(ConfigurationManager.AppSettings["apiURL"], log);

            string interactivity = "service";
            if (_isInteractive)
            {
                interactivity = "interactive";
            }

            log.Info(String.Format("VilicusAgent v{0} starting ({1})", VERSION, interactivity));

            string logging = "Logging to " + log.filename;
            if (_isInteractive) logging += " and stdout";
            log.Info(logging);

            agentConfig = InitialAgentConfig(Convert.ToInt32(ConfigurationManager.AppSettings["apiID"]));
        }

        private API_Agent InitialAgentConfig(int id)
        {
            // Fetch this agent from the manager
            var agent = _api.GetAgent(id);
            if (agent == null)
            {
                log.Error("Agent id=" + id + " does not exist at the Manager");
                System.Environment.Exit(3);
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
            log.Info(String.Format("I am the agent named {0} (id={1}); last checkin was {2}", agent.hostname, id, last_checkin));
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
            
            if (agent.guid != _appGuid)
            {
                log.Info(String.Format("Updating GUID on manager from {0} to {1}", agent.guid, _appGuid));
                agent.guid = _appGuid;
            }

            _api.UpdateAgent(agent);
            return agent;
        }

        private string GetFQDN()
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            return string.IsNullOrWhiteSpace(ipProperties.DomainName) ? ipProperties.HostName
                    : string.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
        }

        private void SendStatus(API_WindowsService service, ServiceController sc, string action_taken)
        {
            var l = new API_WindowsServiceLog();
            l.service = service.resource_uri;
            l.timestamp = DateTime.Now;
            if (sc != null)
            {
                l.actual_status = ServiceState.GetFirstKeyFromValue(sc.Status);
            }
            else
            {
                l.actual_status = "NOT_INSTALLED";
            }
            l.action_taken = action_taken;
            _api.SendServiceLog(l);
        }

        public void DoWork()
        {
            UpdateServices();
            foreach (var service in _services)
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
                    SendStatus(service, sc, "TODO");
                }
                catch (InvalidOperationException)
                {
                    if (service.expected_status != "NOT_INSTALLED")
                    {
                        log.Warn(String.Format("Service {0} is in state NOT_INSTALLED when it was expected to be in state {1}.",
                            service.service_name, service.expected_status));
                    }
                    SendStatus(service, null, "TODO");
                }
            }
        }

        private void UpdateServices()
        {
            var r = _api.GetServices(agentConfig);
            if (r == null)
            {
                log.Error("Failed to fetch list of services from manager.");
                return;
            }

            _services = r.objects;
            if (_services.Count == 0)
            {
                log.Info("There are no configured services for this agent.");
                return;
            }

            log.Debug("Configured services:");
            foreach (var service in _services)
            {
                log.Debug(String.Format("  {0}: {1} (expecting state {2})", service.id, service.service_name, service.expected_status));
            }
        }

        public void DoWorkLoop()
        {
            while (true)
            {
                DoWork();
                Thread.Sleep(agentConfig.check_interval_ms);
            }
        }
    }
}
