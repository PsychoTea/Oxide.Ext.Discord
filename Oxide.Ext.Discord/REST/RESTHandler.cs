using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Ext.Discord.REST
{
    public class RESTHandler
    {
        public List<Bucket> Buckets = new List<Bucket>();

        private string apiKey;
        private Dictionary<string, string> headers;

        public RESTHandler(string apiKey)
        {
            this.apiKey = apiKey;

            headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {this.apiKey}" },
                { "Content-Type", "application/json" }
            };
        }

        public void DoRequest(string url, string method, object data = null, Action callback = null)
        {
            RequestMethod reqMethod = RequestMethod.GET;

            switch (method)
            {
                case "GET": reqMethod = RequestMethod.GET; break;
                case "POST": reqMethod = RequestMethod.POST; break;
                case "PUT": reqMethod = RequestMethod.PUT; break;
                case "PATCH": reqMethod = RequestMethod.PATCH; break;
                case "DELETE": reqMethod = RequestMethod.DELETE; break;
            }

            CreateRequest(reqMethod, url, headers, data, obj => callback?.Invoke());
        }

        public void DoRequest<T>(string url, string method, object data, Action<object> callback)
        {
            RequestMethod reqMethod = RequestMethod.GET;

            switch (method)
            {
                case "GET": reqMethod = RequestMethod.GET; break;
                case "POST": reqMethod = RequestMethod.POST; break;
                case "PUT": reqMethod = RequestMethod.PUT; break;
                case "PATCH": reqMethod = RequestMethod.PATCH; break;
                case "DELETE": reqMethod = RequestMethod.DELETE; break;
            }

            CreateRequest(reqMethod, url, headers, data, response =>
            {
                var callbackObj = JsonConvert.DeserializeObject(response.Data, typeof(T));
                callback?.Invoke(callbackObj);
            });
        }

        public void Shutdown() { }

        public void CreateRequest(RequestMethod method, string url, Dictionary<string, string> headers, object data, Action<RestResponse> callback)
        {
            Interface.Oxide.LogInfo($"Got URL: {url}");

            // this is bad I know, but I'm way too fucking lazy to go 
            // and rewrite every single fucking REST request call
            string[] parts = url.Split('/');

            string route = string.Join("/", parts.Take(3).ToArray()).TrimEnd('/');

            string endpoint = "/" + string.Join("/", parts.Skip(3).ToArray());
            endpoint = endpoint.TrimEnd('/');
            
            var request = new Request(method, route, endpoint, headers, data, callback);
            BucketRequest(request, callback);
        }

        private void BucketRequest(Request request, Action<RestResponse> callback)
        {
            var bucket = Buckets.SingleOrDefault(x => x.Method == request.Method &&
                                                      x.Route == request.Route);

            if (bucket != null)
            {
                bucket.Queue(request);
                return;
            }

            var newBucket = new Bucket(request.Method, request.Route);
            Buckets.Add(newBucket);
            
            newBucket.Queue(request);

            return;
        }
    }
}
