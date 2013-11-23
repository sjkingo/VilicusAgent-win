using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    abstract class ApiRequest
    {
        abstract public Method method { get; }
        public RestRequest restRequest;

        public ApiRequest(string resource, Dictionary<string, string> urlSegments = null)
        {
            restRequest = new RestRequest(resource, method);
            restRequest.RequestFormat = DataFormat.Json;

            // Perform substitution on resource segments
            if (urlSegments != null)
            {
                foreach (var segment in urlSegments)
                {
                    restRequest.AddUrlSegment(segment.Key, segment.Value);
                }
            }
        }

        protected void AddBody(Object body)
        {
            if (method == Method.GET)
            {
                throw new ArgumentException("A body cannot be added when the request is GET", "body");
            }
            restRequest.AddBody(body);
        }
    }
}
