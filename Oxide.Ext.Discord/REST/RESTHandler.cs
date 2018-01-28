namespace Oxide.Ext.Discord.REST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RESTHandler
    {
        private List<Bucket> buckets = new List<Bucket>();

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

        public void Shutdown()
        {
            buckets.ForEach(x =>
            {
                x.Disposed = true;
                x.Close();
            });
        }

        public void DoRequest(string url, RequestMethod method, object data, Action callback)
        {
            CreateRequest(method, url, headers, data, response => callback?.Invoke());
        }

        public void DoRequest<T>(string url, RequestMethod method, object data, Action<T> callback)
        {
            CreateRequest(method, url, headers, data, response =>
            {
                callback?.Invoke(response.ParseData<T>());
            });
        }

        private void CreateRequest(RequestMethod method, string url, Dictionary<string, string> headers, object data, Action<RestResponse> callback)
        {
            // this is bad I know, but I'm way too fucking lazy to go 
            // and rewrite every single fucking REST request call
            string[] parts = url.Split('/');

            string route = string.Join("/", parts.Take(3).ToArray());
            route = route.TrimEnd('/');

            string endpoint = "/" + string.Join("/", parts.Skip(3).ToArray());
            endpoint = endpoint.TrimEnd('/');

            var request = new Request(method, route, endpoint, headers, data, callback);
            BucketRequest(request);
        }

        private void BucketRequest(Request request)
        {
            foreach (var item in new List<Bucket>(buckets).Where(x => x.Disposed))
            {
                buckets.Remove(item);
            }

            var bucket = buckets.SingleOrDefault(x => x.Method == request.Method &&
                                                      x.Route == request.Route);

            if (bucket != null)
            {
                bucket.Queue(request);
                return;
            }

            var newBucket = new Bucket(request.Method, request.Route);
            buckets.Add(newBucket);

            newBucket.Queue(request);
        }
    }
}
