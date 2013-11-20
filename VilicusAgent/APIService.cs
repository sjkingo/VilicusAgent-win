using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class APIService
    {
        public int id { get; set; }
        public string resource_uri { get; set; }
        public string agent { get; set; }
        public string service_name { get; set; }
        public string expected_status { get; set; }
    }

    class APIServiceList
    {
        public List<APIService> objects { get; set; }
    }
}
