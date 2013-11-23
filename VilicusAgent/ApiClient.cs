using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class ApiClient
    {
        private const int sleepTime = 5000; // 5 seconds
        readonly string baseUrl;
        readonly Log log;
        private ConcurrentQueue<ApiRequest> _queue = new ConcurrentQueue<ApiRequest>();
        private BackgroundWorker worker = new BackgroundWorker();

        /// <summary>
        /// Helper methods for interacting with the manager API.
        /// </summary>
        public ApiClient(Log log) {
            this.log = log;
            this.baseUrl = ConfigurationManager.AppSettings["apiURL"];

            this.worker.WorkerReportsProgress = false;
            this.worker.WorkerSupportsCancellation = false;
            this.worker.DoWork += new DoWorkEventHandler(QueueWorker);
            this.worker.RunWorkerAsync();
        }

        /// <summary>
        /// Performs an API request.
        /// </summary>
        /// <param name="request">The request instance to use.</param>
        /// <returns>The response.</returns>
        private IRestResponse SendRequest(ApiRequest request)
        {
            var client = new RestClient(baseUrl);
            log.Debug(String.Format("Executing an API call to {0}/{1} ({2})",
                    client.BaseUrl, request.restRequest.Resource, request.restRequest.Method));

            var response = client.Execute(request.restRequest);
            if (response.ErrorException != null)
            {
                string message = "HTTP error: " + response.StatusDescription;
                throw new ApplicationException(message, response.ErrorException);
            }

            return response;
        }

        /// <summary>
        /// Worker thread started by BackgroundWorker that dequeues and sends requests.
        /// </summary>
        private void QueueWorker(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(sleepTime);
                log.Debug("QueueWorker waking up, " + _queue.Count + " requests to send");

                ApiRequest request;
                while (_queue.TryDequeue(out request))
                {
                    log.Debug("QueueWorker dequeued request: " + request.method + " " + request.restRequest.Resource);
                    SendRequest(request);
                }
                log.Debug("QueueWorker queue is now empty, going to sleep for " + sleepTime + " ms");
            }
        }

        /// <summary>
        /// Enqueue a new request to send. This is not valid for GET requests. Use GetRequest() for that.
        /// </summary>
        /// <param name="request">The request to send.</param>
        public void Enqueue(ApiRequest request)
        {
            if (request.method == Method.GET)
            {
                throw new ArgumentException("GET requests cannot be enqueued. Try GetRequest().", "request");
            }
            this._queue.Enqueue(request);
            log.Debug("Enqueued new request: " + request.method + " " + request.restRequest.Resource);
        }

        /// <summary>
        /// Make a new GET request and block for response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response to.</typeparam>
        /// <param name="request">The request to send.</param>
        /// <returns>The response deserialized into a new instance of T.</returns>
        public T GetRequest<T>(ApiRequest request)
            where T : new()
        {
            if (request.method != Method.GET)
            {
                throw new ArgumentException("Only GET requests can be send through GetRequest(). Try Enqueue().", "request");
            }

            var response = SendRequest(request);
            var json = new RestSharp.Deserializers.JsonDeserializer();
            return json.Deserialize<T>(response);
        }

        #region GET requests that return data

        public API_Agent GetAgent(int id)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", id.ToString());
            var req = new ApiRequest_Get("agent/{id}/", urlSegments: segments);
            return GetRequest<API_Agent>(req);
        }

        public APIServiceList GetServices(API_Agent agent)
        {
            var qs = new Dictionary<string, string>();
            qs.Add("agent__id", agent.id.ToString());
            var req = new ApiRequest_Get("windowsservice/", queryStringArguments: qs);
            return GetRequest<APIServiceList>(req);
        }

        #endregion

        #region POST/PUT requests for sending data to the manager

        public void SendWindowsServiceLog(API_WindowsServiceLog l)
        {
            var req = new ApiRequest_Post("windowsservicelog/", l);
            Enqueue(req);
        }

        public void SendPerfLog(API_PerformanceLogEntry p)
        {
            var req = new ApiRequest_Post("performancelogentry/", p);
            Enqueue(req);
        }

        public void SendAgentUpdate(API_Agent agent)
        {
            var segments = new Dictionary<string, string>();
            segments.Add("id", agent.id.ToString());
            var req = new ApiRequest_Put("agent/{id}/", agent, urlSegments: segments);
            Enqueue(req);
        }

        #endregion
    }
}
