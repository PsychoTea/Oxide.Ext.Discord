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
            
            WebRequest req = null;
            try
            {
                req = WebRequest.Create(RequestURL);
                req.Method = Method.ToString();
                req.Timeout = 5000;
            }
            catch (NullReferenceException nre)
            {
                Interface.Oxide.LogException($"Exception thrown in Request.Fire (request creation)", nre);
                Interface.Oxide.LogError($"req == null {req == null}");
                Interface.Oxide.LogError($"method == null {Method.ToString() == null}");

                this.Close(false);
                return;
            }

            try
            {
                if (Headers != null)
                {
                    req.SetRawHeaders(Headers);
                }
            }
            catch (NullReferenceException nre)
            {
                Interface.Oxide.LogException($"Exception thrown in Request.Fire (setting headers)", nre);
                Interface.Oxide.LogError($"req == null {req == null}");
                Interface.Oxide.LogError($"Headers == null {Headers == null}");

                this.Close(false);
                return;
            }

            if (Data != null)
            {
                string contents = JsonConvert.SerializeObject(Data, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                byte[] bytes = null;
                try
                {
                    bytes = Encoding.ASCII.GetBytes(contents);
                }
                catch (NullReferenceException nre)
                {
                    Interface.Oxide.LogException($"Exception thrown in Request.Fire (getting contents bytes)", nre);
                    Interface.Oxide.LogError($"contents == null {contents == null}");

                    this.Close(false);
                    return;
                }

                try
                {
                    req.ContentLength = bytes.Length;
                }
                catch (NullReferenceException nre)
                {
                    Interface.Oxide.LogException($"Exception thrown in Request.Fire (setting content length)", nre);
                    Interface.Oxide.LogError($"req == null {req == null}");
                    Interface.Oxide.LogError($"bytes == null {bytes == null}");

                    this.Close(false);
                    return;
                }

                try
                {
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (NullReferenceException nre)
                {
                    Interface.Oxide.LogException($"Exception thrown in Request.Fire (writing request stream)", nre);
                    Interface.Oxide.LogError($"req == null {req == null}");
                    Interface.Oxide.LogError($"bytes == null {bytes == null}");

                    this.Close(false);
                    return;
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

                string message = null;
                try
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd().Trim();
                    }
                }
                catch (NullReferenceException nre)
                {
                    Interface.Oxide.LogException($"Exception thrown in Request.Fire (reading error response stream)", nre);
                    Interface.Oxide.LogError($"ex == null {ex == null}");
                    Interface.Oxide.LogError($"httpResponse == null {httpResponse == null}");

                    this.Close(false);
                    return;
                }

                this.Response = new RestResponse(message);

                this.ParseHeaders(httpResponse.Headers, this.Response);

                Interface.Oxide.LogWarning($"[Discord Ext] An error occured whilst submitting a request to {req.RequestUri} (code {httpResponse.StatusCode}): {message}");

                if ((int)httpResponse.StatusCode == 429)
                {
                    Interface.Oxide.LogWarning($"[Discord Ext] Ratelimit info: remaining: {bucket.Remaining}, limit: {bucket.Limit}, reset: {bucket.Reset}, time now: {bucket.TimeSinceEpoch()}");
                }

                httpResponse.Close();

                bool shouldRemove = (int)httpResponse.StatusCode != 429;
                this.Close(shouldRemove);

                return;
            }

            string output = "";

            try
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    output = reader.ReadToEnd().Trim();
                }
            }
            catch (NullReferenceException nre)
            {
                Interface.Oxide.LogException($"Exception thrown in Request.Fire (reading response stream contents)", nre);
                Interface.Oxide.LogError($"response == null {response == null}");

                this.Close(false);
                return;
            }

            this.Response = new RestResponse(output);

            try
            {
                this.ParseHeaders(response.Headers, this.Response);
            }
            catch (NullReferenceException nre)
            {
                Interface.Oxide.LogException($"Exception thrown in Request.Fire (parsing headers)", nre);
                Interface.Oxide.LogError($"response == null {response == null}");
                Interface.Oxide.LogError($"response.Headers == null {response?.Headers == null}");
                Interface.Oxide.LogError($"response == null {this.Response == null}");

                this.Close(false);
                return;
            }

            try
            {
                response.Close();
            }
            catch (NullReferenceException nre)
            {
                Interface.Oxide.LogException($"Exception thrown in Request.Fire (closing request)", nre);
                Interface.Oxide.LogError($"response == null {response == null}");

                this.Close(false);
                return;
            }

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

            //Interface.Oxide.LogInfo($"Recieved ratelimit deets: {bucket.Limit}, {bucket.Remaining}, {bucket.Reset}, time now: {bucket.TimeSinceEpoch()}");
            //Interface.Oxide.LogInfo($"Time until reset: {(bucket.Reset - (int)bucket.TimeSinceEpoch())}");
        }
    }
}
