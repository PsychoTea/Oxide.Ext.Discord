namespace Oxide.Ext.Discord.REST
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json;
    using Oxide.Core;
    using Oxide.Core.Libraries;
    using Oxide.Ext.Discord.DiscordObjects;

    public class Request
    {
        private const string URLBase = "https://discordapp.com/api";

        public RequestMethod Method { get; }

        public string Route { get; }

        public string Endpoint { get; }

        public Dictionary<string, string> Headers { get; }

        public object Data { get; }

        public RestResponse Response { get; private set; }

        public Action<RestResponse> Callback { get; }

        public bool InProgress { get; private set; } = false;

        private Bucket bucket;

        public Request(RequestMethod method, string route, string endpoint, Dictionary<string, string> headers, object data, Action<RestResponse> callback)
        {
            this.Method = method;
            this.Route = route;
            this.Endpoint = endpoint;
            this.Headers = headers;
            this.Data = data;
            this.Callback = callback;
        }

        public void Fire(Bucket bucket)
        {
            this.bucket = bucket;
            this.InProgress = true;

            string url = URLBase + Route + Endpoint;

            var req = WebRequest.Create(url);
            req.Method = Method.ToString();

            if (Headers != null)
            {
                req.SetRawHeaders(Headers);
            }

            if (Data != null)
            {
                string contents = JsonConvert.SerializeObject(Data, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                byte[] bytes = Encoding.ASCII.GetBytes(contents);
                req.ContentLength = bytes.Length;

                using (var stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            HttpWebResponse response;
            try
            {
                response = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                var httpResponse = ex.Response as HttpWebResponse;
                string message = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Interface.Oxide.LogWarning($"[Discord Ext] An error occured whilst submitting a request to {req.RequestUri} (code {httpResponse.StatusCode}): {message}");
                return;
            }
            
            string output;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                output = reader.ReadToEnd().Trim();
            }

            ParseHeaders(response.Headers, output);

            this.Response = new RestResponse(output);

            Callback?.Invoke(this.Response);

            this.bucket.Remove(this);

            this.InProgress = false;
        }

        private void ParseHeaders(WebHeaderCollection headers, string response)
        {
            string rateRetryAfterHeader = headers.Get("Retry-After");
            string rateLimitGlobalHeader = headers.Get("X-RateLimit-Global");

            if (!string.IsNullOrEmpty(rateRetryAfterHeader) &&
                !string.IsNullOrEmpty(rateLimitGlobalHeader))
            {
                int rateRetryAfter;
                bool rateLimitGlobal;

                if (int.TryParse(rateRetryAfterHeader, out rateRetryAfter) &&
                    bool.TryParse(rateLimitGlobalHeader, out rateLimitGlobal) &&
                    rateLimitGlobal)
                {
                    RateLimit limit = JsonConvert.DeserializeObject<RateLimit>(response);

                    if (limit.global)
                    {
                        GlobalRateLimit.Reached(rateRetryAfter);
                    }
                }
            }

            string rateLimitHeader = headers.Get("X-RateLimit-Limit");
            string rateRemainingHeader = headers.Get("X-RateLimit-Remaining");
            string rateResetHeader = headers.Get("X-RateLimit-Reset");

            if (!string.IsNullOrEmpty(rateLimitHeader) &&
                !string.IsNullOrEmpty(rateResetHeader) &&
                !string.IsNullOrEmpty(rateResetHeader))
            {
                int rateLimit, rateRemaining, rateReset;

                if (int.TryParse(rateLimitHeader, out rateLimit) &&
                    int.TryParse(rateRemainingHeader, out rateRemaining) &&
                    int.TryParse(rateResetHeader, out rateReset))
                {
                    bucket.Limit = rateLimit;
                    bucket.Remaining = rateRemaining;
                    bucket.Reset = rateReset;
                }
            }
        }
    }
}
