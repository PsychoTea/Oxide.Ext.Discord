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

            public object DoRequest()
            {
                var req = WebRequest.Create($"{URLBase}{URL}");
                req.SetRawHeaders(Headers);
                req.Method = Method;

                if (Data != null)
                {
                    string contents = JsonConvert.SerializeObject(Data);
                    byte[] bytes = Encoding.ASCII.GetBytes(contents);
                    req.ContentLength = bytes.Length;

                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }

                var response = req.GetResponse() as HttpWebResponse;
                var reader = new StreamReader(response.GetResponseStream());
                string output = reader.ReadToEnd().Trim();

                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    Interface.Oxide.LogWarning($"An error occured whilst submitting a request to {req.RequestUri} (code {response.StatusCode}): {output}");
                    return null;
                }

                if (ReturnType == typeof(void))
                {
                    Callback?.Invoke();
                    return null;
                }

                var retObj = JsonConvert.DeserializeObject(output, ReturnType);
                CallbackObj?.Invoke(retObj);
                return retObj;
            }
        }

        private static class ThreadManager
        {
            static Thread thread = null;
            static List<RequestObject> PendingRequests = new List<RequestObject>();

            public static void Start()
            {
                if (thread != null) return;

                thread = new Thread(() => RunThread());
                thread.Start();
            }

            public static void Stop()
            {
                thread?.Abort();
                thread = null;
            }

            public static void AddRequest(RequestObject newRequest)
            {
                if (thread == null) ThreadManager.Start();
                PendingRequests.Add(newRequest);
            }

            static void RunThread()
            {
                while (thread.ThreadState != ThreadState.AbortRequested)
                {
                    if (PendingRequests.Count > 0)
                    {
                        var currentRequest = PendingRequests.First();
                        currentRequest.DoRequest();
                        PendingRequests.Remove(currentRequest);
                    }
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

            ThreadManager.Start();
        }

        public void Shutdown()
        {
            ThreadManager.Stop();
        }

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

        public T DoRequestNow<T>(string URL, string method)
        {
            var reqObj = new RequestObject(URL, method, typeof(T));
            return (T)reqObj.DoRequest();
        }
    }
}