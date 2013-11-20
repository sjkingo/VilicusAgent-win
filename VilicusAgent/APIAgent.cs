using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    [Serializable, SerializeAs(Name = "object")]
    class APIAgent
    {
        public int id { get; set; }
        public string hostname { get; set; }
        public int check_interval_ms { get; set; }
        public DateTime? last_checkin { get; set; }
        public string version { get; set; }
        public string guid { get; set; }
    }
}
