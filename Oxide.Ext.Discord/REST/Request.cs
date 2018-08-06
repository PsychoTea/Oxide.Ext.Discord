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

        private const double RequestMaxLength = 10d;

        public RequestMethod Method { get; }

        public string Route { get; }

        public string Endpoint { get; }

        public string RequestURL => URLBase + Route + Endpoint;

        public Dictionary<string, string> Headers { get; }

        public object Data { get; }

        public RestResponse Response { get; private set; }

        public Action<RestResponse> Callback { get; }

        public DateTime? StartTime { get; private set; } = null;

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
            this.StartTime = DateTime.UtcNow;

            var req = WebRequest.Create(RequestURL);
            req.Method = Method.ToString();
            req.ContentType = "application/json";
            req.Timeout = 5000;

            if (this.Headers != null)
            {
                req.SetRawHeaders(this.Headers);
            }

            if (this.Data != null)
            {
                WriteRequestData(req, this.Data);
            }
            else
            {
                req.ContentLength = 0;
            }


            HttpWebResponse response;
            try
            {
                response = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                var httpResponse = ex.Response as HttpWebResponse;

                if (httpResponse == null)
                {
                    Interface.Oxide.LogException($"[Discord Ext] A web request exception occured (internal error).", ex);
                    Interface.Oxide.LogError($"[Discord Ext] Request URL: [{Method.ToString()}] {RequestURL}");
                    Interface.Oxide.LogError($"[Discord Ext] Exception message: {ex.Message}");

                    this.Close(false);
                    return;
                }

                string message = this.ParseResponse(ex.Response);

                Interface.Oxide.LogWarning($"[Discord Ext] An error occured whilst submitting a request to {req.RequestUri} (code {httpResponse.StatusCode}): {message}");

                if ((int)httpResponse.StatusCode == 429)
                {
                    Interface.Oxide.LogWarning($"[Discord Ext] Ratelimit info: remaining: {bucket.Remaining}, limit: {bucket.Limit}, reset: {bucket.Reset}, time now: {Helpers.Time.TimeSinceEpoch()}");
                }

                httpResponse.Close();

                bool shouldRemove = (int)httpResponse.StatusCode != 429;
                this.Close(shouldRemove);

                return;
            }

            this.ParseResponse(response);

            try
            {
                Callback?.Invoke(this.Response);
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException("[Discord Ext] Request callback raised an exception", ex);
            }
            finally
            {
                this.Close();
            }
        }

        public void Close(bool remove = true)
        {
            if (remove)
            {
                this.bucket.Remove(this);
            }

            this.InProgress = false;
        }

        public bool HasTimedOut()
        {
            if (!this.InProgress || StartTime == null) return false;

            var timeSpan = DateTime.UtcNow - StartTime;

            return timeSpan.HasValue && (timeSpan.Value.TotalSeconds > RequestMaxLength);
        }

        private void WriteRequestData(WebRequest request, object data)
        {
            string contents = JsonConvert.SerializeObject(Data, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private string ParseResponse(WebResponse response)
        {
            string message;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                message = reader.ReadToEnd().Trim();
            }

            this.Response = new RestResponse(message);

            this.ParseHeaders(response.Headers, this.Response);

            return message;
        }

        private void ParseHeaders(WebHeaderCollection headers, RestResponse response)
        {
            string rateRetryAfterHeader = headers.Get("Retry-After");
            string rateLimitGlobalHeader = headers.Get("X-RateLimit-Global");

            if (!string.IsNullOrEmpty(rateRetryAfterHeader) &&
                !string.IsNullOrEmpty(rateLimitGlobalHeader) &&
                int.TryParse(rateRetryAfterHeader, out int rateRetryAfter) &&
                bool.TryParse(rateLimitGlobalHeader, out bool rateLimitGlobal) &&
                rateLimitGlobal)
            {
                var limit = response.ParseData<RateLimit>();

                if (limit.global)
                {
                    GlobalRateLimit.Reached(rateRetryAfter);
                }
            }

            string rateLimitHeader = headers.Get("X-RateLimit-Limit");
            string rateRemainingHeader = headers.Get("X-RateLimit-Remaining");
            string rateResetHeader = headers.Get("X-RateLimit-Reset");

            if (!string.IsNullOrEmpty(rateLimitHeader) &&
                int.TryParse(rateLimitHeader, out int rateLimit))
            {
                bucket.Limit = rateLimit;
            }

            if (!string.IsNullOrEmpty(rateRemainingHeader) &&
                int.TryParse(rateRemainingHeader, out int rateRemaining))
            {
                bucket.Remaining = rateRemaining;
            }

            if (!string.IsNullOrEmpty(rateResetHeader) &&
                int.TryParse(rateResetHeader, out int rateReset))
            {
                bucket.Reset = rateReset;
            }

            ////Interface.Oxide.LogInfo($"Recieved ratelimit deets: {bucket.Limit}, {bucket.Remaining}, {bucket.Reset}, time now: {bucket.TimeSinceEpoch()}");
            ////Interface.Oxide.LogInfo($"Time until reset: {(bucket.Reset - (int)bucket.TimeSinceEpoch())}");
        }
    }
}
