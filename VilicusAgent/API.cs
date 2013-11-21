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
        readonly int _authID;
        readonly string _authKey;
        private Log log;

        public API(string baseURL, int authID, Log log) {
            this._baseURL = baseURL;
            this._authID = authID;
            this.log = log;
        }

        /// <summary>
        /// Low-level execution of a REST request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="request">The pre-filled request object to use.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(_baseURL);
            //client.Authenticator = new HttpBasicAuthenticator(_authUUID, _authKey);
            log.Debug(String.Format("Executing an API call to {0}/{1} ({2})", 
                    client.BaseUrl, request.Resource, request.Method));

            var response = client.Execute<T>(request);
            if (response.ErrorException != null)
            {
                string message = "HTTP error from manager: " + response.StatusDescription;
                var ex = new ApplicationException(message, response.ErrorException);
                throw ex;
            }
            //Console.Write(response.Content);
            return response.Data;
        }

        /// <summary>
        /// Perform an API request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="method">The HTTP method to use for this request.</param>
        /// <param name="resource">The URL path of the HTTP request.</param>
        /// <param name="segments">Key/value pairs of items to substitute in the resource.</param>
        /// <param name="body">Object to serialize and add to the body of the request.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T _APIRequest<T>(Method method, string resource, Dictionary<string, string> segments,
            Object body) where T : new()
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            foreach (var entry in segments)
            {
                request.AddUrlSegment(entry.Key, entry.Value);
            }
            request.AddBody(body);
            return Execute<T>(request);
        }

        /// <summary>
        /// Perform an API request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="method">The HTTP method to use for this request.</param>
        /// <param name="resource">The URL path of the HTTP request.</param>
        /// <param name="body">Object to serialize and add to the body of the request.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T _APIRequest<T>(Method method, string resource, Object body) where T : new()
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(body);
            return Execute<T>(request);
        }

        /// <summary>
        /// Perform an API request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="method">The HTTP method to use for this request.</param>
        /// <param name="resource">The URL path of the HTTP request.</param>
        /// <param name="segments">Key/value pairs of items to substitute in the resource.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T _APIRequest<T>(Method method, string resource, Dictionary<string, string> segments,
            ParameterType segmentType)
            where T : new()
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            foreach (var entry in segments)
            {
                request.AddParameter(entry.Key, entry.Value, segmentType);
            }
            return Execute<T>(request);
        }

        /// <summary>
        /// Perform an API request.
        /// </summary>
        /// <typeparam name="T">The return type to deserialize the response to.</typeparam>
        /// <param name="method">The HTTP method to use for this request.</param>
        /// <param name="resource">The URL path of the HTTP request.</param>
        /// <returns>Response deserialized into a new object of type T</returns>
        private T _APIRequest<T>(Method method, string resource)
            where T : new()
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            return Execute<T>(request);
        }

        public APIAgent GetAgent(int id)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", id.ToString());

            try
            {
                return _APIRequest<APIAgent>(Method.GET, "agent/{id}/", segments, ParameterType.UrlSegment);
            }
            catch (ApplicationException e)
            {
                log.Error(e.Message);
                return null;
            }
        }

        public APIAgent UpdateAgent(APIAgent agent)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", agent.id.ToString());

            try {
                return _APIRequest<APIAgent>(Method.PUT, "agent/{id}/", segments, agent);
            }
            catch (ApplicationException e)
            {
                log.Error(e.Message);
                return null;
            }
        }

        public APIServiceList GetServices(APIAgent agent)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("agent__id", agent.id.ToString());

            try
            {
                return _APIRequest<APIServiceList>(Method.GET, "windowsservice/", segments, ParameterType.QueryString);
            }
            catch (ApplicationException e)
            {
                log.Error(e.Message);
                return null;
            }
        }

        public APIServiceLog SendServiceLog(APIServiceLog l)
        {
            try
            {
                return _APIRequest<APIServiceLog>(Method.POST, "windowsservicelog/", l);
            }
            catch (ApplicationException e)
            {
                log.Error(e.Message);
                return null;
            }
        }
    }
}
