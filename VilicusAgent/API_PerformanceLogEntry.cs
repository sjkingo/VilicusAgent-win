using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class API_PerformanceLogEntry
    {
        public string agent { get; set; }
        public DateTime timestamp { get; set; }
        public int cpu_usage { get; set; }
    }
}
