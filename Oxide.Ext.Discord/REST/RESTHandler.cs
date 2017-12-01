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

        public RESTHandler(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void DoRequest(string url, string method, object data = null, Action callback = null)
        {
            Interface.Oxide.LogInfo($"RESTHandler.DoRequest #2 has been called on {url}");

            RequestMethod reqMethod = RequestMethod.GET;

            switch (method)
            {
                case "GET": reqMethod = RequestMethod.GET; break;
                case "POST": reqMethod = RequestMethod.POST; break;
                case "PUT": reqMethod = RequestMethod.PUT; break;
                case "PATCH": reqMethod = RequestMethod.PATCH; break;
                case "DELETE": reqMethod = RequestMethod.DELETE; break;
            }

            CreateRequest(reqMethod, url, new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                {  "Content-Type", "application/json" }
            }, data, obj => callback?.Invoke());
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

            CreateRequest(reqMethod, url, new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                {  "Content-Type", "application/json" }
            }, data, response =>
            {
                var callbackObj = JsonConvert.DeserializeObject(response.Data, typeof(T));
                callback?.Invoke(callbackObj);
            });
        }

        public void Shutdown() { }

        public void CreateRequest(RequestMethod method, string url, Dictionary<string, string> headers, object data, Action<RestResponse> callback)
        {
            Interface.Oxide.LogInfo($"Got URL: {url}");

            var request = new Request(method, url, url, headers, data);
            BucketRequest(request, callback);
        }

        private void BucketRequest(Request request, Action<RestResponse> callback)
        {
            var bucket = Buckets.SingleOrDefault(x => x.Method == request.Method &&
                                                      x.Route == request.Route);

            if (bucket != null)
            {
                bucket.Add(request);
                bucket.Fire(request, callback);
                return;
            }

            var newBucket = new Bucket(request.Method, request.Route);
            Buckets.Add(newBucket);

            newBucket.Add(request);
            newBucket.Fire(request, callback);
            return;
        }
    }
}
