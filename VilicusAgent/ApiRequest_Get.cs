using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class ApiRequest_Get : ApiRequest
    {
        /// <summary>
        /// The HTTP method of this request.
        /// </summary>
        override public Method method
        {
            get { return Method.GET; }
        }

        /// <summary>
        /// A GET request.
        /// </summary>
        /// <param name="resource">The resource to send the request to.</param>
        /// <param name="urlSegments">Optional. Key/value pairs to substitute in the resource segments.</param>
        /// <param name="queryStringArguments">Optional. Key/value pairs to use to construct a query string.</param>
        public ApiRequest_Get(string resource, Dictionary<string, string> urlSegments = null,
            Dictionary<string, string> queryStringArguments = null)
            : base(resource, urlSegments)
        {
            if (queryStringArguments != null)
            {
                foreach (var segment in queryStringArguments)
                {
                    restRequest.AddParameter(segment.Key, segment.Value, ParameterType.QueryString);
                }
            }
        }
    }
}
