using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class API_WindowsServiceLog
    {
        public string service { get; set; }
        public DateTime timestamp { get; set; }
        public string expected_status { get; set; }
        public string actual_status { get; set; }
        public string action_taken { get; set; }
        public string comments { get; set; }
    }
}
