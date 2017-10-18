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

namespace Oxide.Ext.Discord.WebSockets
{
    public class RESTHandler
    {
        private class RequestObject
        {
            public string URL;
            string Method;
            object Data = null;
            Action Callback = null;
            Action<object> CallbackObj = null;
            Type ReturnType = typeof(void);

            public RequestObject(string url, string method, Type returnType = null)
            {
                URL = url;
                Method = method;
                ReturnType = returnType;
            }

            public RequestObject(string url, string method, object data = null, Action callback = null, Type returnType = null)
            {
                URL = url;
                Method = method;
                Data = data;
                Callback = callback;
                ReturnType = returnType;
            }

            public RequestObject(string url, string method, object data = null, Action<object> callback = null, Type returnType = null)
            {
                URL = url;
                Method = method;
                Data = data;
                CallbackObj = callback;
                ReturnType = (returnType == null) ? typeof(void) : returnType;
            }

            public void DoRequest()
            {
                var req = WebRequest.Create($"{URLBase}{URL}");
                req.SetRawHeaders(Headers);
                req.Method = Method;

                if (Data != null)
                {
                    string contents = JsonConvert.SerializeObject(Data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
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

                var reader = new StreamReader(response.GetResponseStream());
                string output = reader.ReadToEnd().Trim();

                if (ReturnType == typeof(void))
                {
                    Callback?.Invoke();
                    return;
                }

                var retObj = JsonConvert.DeserializeObject(output, ReturnType);
                CallbackObj?.Invoke(retObj);
            }
        }

        private static class ThreadManager
        {
            static Thread Thread = null;
            static List<RequestObject> PendingRequests = new List<RequestObject>();

            public static void Start()
            {
                if (IsRunning())
                {
                    Interface.Oxide.LogWarning($"[Discord Ext] RESTHandler thread was started whilst already running!");
                    return;
                }

                Thread = new Thread(() => RunThread());
                Thread.Start();
            }

            public static bool IsRunning() => Thread != null && Thread.ThreadState != ThreadState.Stopped;

            public static void Stop()
            {
                Thread?.Abort();
                Thread = null;
            }

            public static void AddRequest(RequestObject newRequest)
            {
                PendingRequests.Add(newRequest);

                if (!IsRunning())
                {
                    ThreadManager.Start();
                }
            }

            private static void RunThread()
            {
                while (PendingRequests.Count > 0)
                {
                    var currentRequest = PendingRequests.First();
                    currentRequest.DoRequest();
                    PendingRequests.Remove(currentRequest);
                }
            }
        }

        private const string URLBase = "https://discordapp.com/api";
        private static Dictionary<string, string> Headers;

        public RESTHandler(string apiKey)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                { "Content-Type", "application/json" }
            };
        }

        public void Shutdown() => ThreadManager.Stop();

        public void DoRequest(string URL, string method, object data = null, Action callback = null)
        {
            var reqObj = new RequestObject(URL, method, data, callback);
            ThreadManager.AddRequest(reqObj);
        }

        public void DoRequest<T>(string URL, string method, object data = null, Action<object> callback = null)
        {
            var reqObj = new RequestObject(URL, method, data, callback, typeof(T));
            ThreadManager.AddRequest(reqObj);
        }
    }
}