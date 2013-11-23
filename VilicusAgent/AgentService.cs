using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace VilicusAgent
{
    public partial class AgentService : ServiceBase
    {
        private Agent agent;
        private Timer timer;

        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            agent = new Agent(false);
            timer = new Timer(agent.agent.check_interval_ms);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(WorkerTick);
            timer.Start();
        }

        protected override void OnStop()
        {
            agent.log.Info("Service stopped");
        }

        private void WorkerTick(object sender, ElapsedEventArgs e)
        {
            agent.DoWork();
        }
    }
}
