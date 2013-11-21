using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class API
    {
        readonly string _baseURL;
        private Log _log;

        /// <summary>
        /// Class for interacting with the manager API.
        /// </summary>
        /// <param name="baseURL">The base URL to find the manager API at.</param>
        /// <param name="log">Instance of the log class that messages can be written to.</param>
        public API(string baseURL, Log log) {
            this._baseURL = baseURL;
            this._log = log;
        }

        /// <summary>
        /// Low-level execution of a REST request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="request">The pre-filled request object to use.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T _Execute<T>(RestRequest request)
            where T : new()
        {
            var client = new RestClient(_baseURL);
            _log.Debug(String.Format("Executing an API call to {0}/{1} ({2})", 
                    client.BaseUrl, request.Resource, request.Method));

            var response = client.Execute<T>(request);
            if (response.ErrorException != null)
            {
                string message = "HTTP error from manager: " + response.StatusDescription;
                throw new ApplicationException(message, response.ErrorException);
            }
            return response.Data;
        }

        /// <summary>
        /// Make an API request to the manager instance and return the response as type T
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response into.</typeparam>
        /// <param name="method">The HTTP method (GET, PUT, POST) to use.</param>
        /// <param name="resource">The URI resource (without a leading slash) to request. Segments to replace (see urlSegments param) may be used like so: {id}.</param>
        /// <param name="urlSegments">Optional. A mapping of key/value pairs to substitute in the resource param.</param>
        /// <param name="queryStringSegments">Optional. A mapping of key/value pairs to construct a query string with.</param>
        /// <param name="body">Optional. An object to be serialized and placed into the body of the request. Not valid when method=GET.</param>
        /// <returns>Response deserialized into a new object of type T.</returns>
        private T APIRequest<T>(Method method, string resource,
            Dictionary<string, string> urlSegments = null,
            Dictionary<string, string> queryStringSegments = null,
            Object body = null)
            where T : new()
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            // Perform substitution on resource segments
            if (urlSegments != null)
            {
                foreach (var segment in urlSegments)
                {
                    request.AddUrlSegment(segment.Key, segment.Value);
                }
            }

            // Add the query string segments
            if (queryStringSegments != null)
            {
                foreach (var segment in queryStringSegments)
                {
                    request.AddParameter(segment.Key, segment.Value, ParameterType.QueryString);
                }
            }

            // Add the body
            if (body != null)
            {
                if (method == Method.GET)
                {
                    throw new ArgumentException("Specifying a body with method=GET is not valid.", "body");
                }
                request.AddBody(body);
            }

            return _Execute<T>(request);
        }

        public APIAgent GetAgent(int id)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", id.ToString());

            try
            {
                return APIRequest<APIAgent>(Method.GET, "agent/{id}/", urlSegments: segments);
            }
            catch (ApplicationException e)
            {
                _log.Error(e.Message);
                return null;
            }
        }

        public APIAgent UpdateAgent(APIAgent agent)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", agent.id.ToString());

            try {
                return APIRequest<APIAgent>(Method.PUT, "agent/{id}/", urlSegments: segments, body: agent);
            }
            catch (ApplicationException e)
            {
                _log.Error(e.Message);
                return null;
            }
        }

        public APIServiceList GetServices(APIAgent agent)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("agent__id", agent.id.ToString());

            try
            {
                return APIRequest<APIServiceList>(Method.GET, "windowsservice/", queryStringSegments: segments);
            }
            catch (ApplicationException e)
            {
                _log.Error(e.Message);
                return null;
            }
        }

        public APIServiceLog SendServiceLog(APIServiceLog l)
        {
            try
            {
                return APIRequest<APIServiceLog>(Method.POST, "windowsservicelog/", body: l);
            }
            catch (ApplicationException e)
            {
                _log.Error(e.Message);
                return null;
            }
        }
    }
}
