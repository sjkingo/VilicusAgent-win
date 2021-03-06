﻿using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class ApiRequest_Post : ApiRequest
    {
        /// <summary>
        /// The HTTP method of this request.
        /// </summary>
        override public Method method
        {
            get { return Method.POST; }
        }

        /// <summary>
        /// A POST request.
        /// </summary>
        /// <param name="resource">The resource to send the request to.</param>
        /// <param name="body">An object to serialize and send as part of the request.</param>
        /// <param name="urlSegments">Optional. Key/value pairs to substitute in the resource segments.</param>
        public ApiRequest_Post(string resource, Object body, Dictionary<string, string> urlSegments = null)
            : base(resource, urlSegments)
        {
            AddBody(body);
        }
    }
}
