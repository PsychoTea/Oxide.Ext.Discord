namespace Oxide.Ext.Discord.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;
    using Oxide.Core;
    using Oxide.Core.Libraries;

    public class RESTHandler
    {
        private static class ThreadManager
        {
            private static Thread thread = null;

            private static List<RequestObject> pendingRequests = new List<RequestObject>();

            public static bool IsRunning() => thread != null && thread.ThreadState != ThreadState.Stopped;

            public static void Stop()
            {
                thread?.Abort();
                thread = null;
            }

            public static void AddRequest(RequestObject newRequest)
            {
                pendingRequests.Add(newRequest);

                if (!IsRunning())
                {
                    ThreadManager.Start();
                }
            }

            private static void Start()
            {
                if (IsRunning())
                {
                    Interface.Oxide.LogWarning($"[Discord Ext] RESTHandler thread was started whilst already running!");
                    return;
                }

                thread = new Thread(() => RunThread());
                thread.Start();
            }

            private static void RunThread()
            {
                while (pendingRequests.Count > 0)
                {
                    var currentRequest = pendingRequests.First();
                    currentRequest.DoRequest();
                    pendingRequests.Remove(currentRequest);
                }
            }
        }

        private class RequestObject
        {
            public string URL { get; private set; }

            private string method;

            private Dictionary<string, string> headers;

            private object data = null;

            private Action callback = null;

            private Action<object> callbackObj = null;

            private Type returnType = typeof(void);

            public RequestObject(string url, string method, Dictionary<string, string> headers = null, Type returnType = null)
            {
                URL = url;
                this.method = method;
                this.headers = headers;
                this.returnType = returnType;
            }

            public RequestObject(string url, string method, Dictionary<string, string> headers = null, object data = null, Action callback = null, Type returnType = null)
            {
                URL = url;
                this.method = method;
                this.headers = headers;
                this.data = data;
                this.callback = callback;
                this.returnType = returnType;
            }

            public RequestObject(string url, string method, Dictionary<string, string> headers = null, object data = null, Action<object> callback = null, Type returnType = null)
            {
                URL = url;
                this.method = method;
                this.headers = headers;
                this.data = data;
                callbackObj = callback;
                this.returnType = (returnType == null) ? typeof(void) : returnType;
            }

            public void DoRequest()
            {
                var req = WebRequest.Create($"{URLBase}{URL}");
                req.Method = method;

                if (headers != null)
                {
                    req.SetRawHeaders(headers);
                }

                if (data != null)
                {
                    string contents = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
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

                if (returnType == typeof(void))
                {
                    callback?.Invoke();
                    return;
                }

                var retObj = JsonConvert.DeserializeObject(output, returnType);
                callbackObj?.Invoke(retObj);
            }
        }

        private const string URLBase = "https://discordapp.com/api";
        private Dictionary<string, string> headers;

        public RESTHandler(string apiKey)
        {
            headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                { "Content-Type", "application/json" }
            };
        }

        public void Shutdown() => ThreadManager.Stop();

        public void DoRequest(string url, string method, object data = null, Action callback = null)
        {
            var reqObj = new RequestObject(url, method, headers, data, callback);
            ThreadManager.AddRequest(reqObj);
        }

        public void DoRequest<T>(string url, string method, object data = null, Action<object> callback = null)
        {
            var reqObj = new RequestObject(url, method, headers, data, callback, typeof(T));
            ThreadManager.AddRequest(reqObj);
        }
    }
}