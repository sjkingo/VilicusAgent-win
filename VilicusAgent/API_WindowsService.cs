using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class API_WindowsService
    {
        public int id { get; set; }
        public string resource_uri { get; set; }
        public string agent { get; set; }
        public string service_name { get; set; }
        public string expected_status { get; set; }
    }

    class APIServiceList
    {
        public List<API_WindowsService> objects { get; set; }
    }
}
