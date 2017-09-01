using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core;
using System.Threading;
using System;
using System.Linq;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class RESTHandler
    {
        class RequestObject
        {
            string URL;
            string Method;
            object Data = null;
            Action<object> Callback = null;
            Type ReturnType = typeof(void);
            
            public RequestObject(string url, string method, object data = null, Action<object> callback = null, Type returnType = null)
            {
                URL = url;
                Method = method;
                Data = data;
                Callback = callback;
                ReturnType = (returnType == null) ? typeof(void) : returnType;
            }

            public void DoRequest()
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
                    return;
                }

                if (ReturnType == typeof(void) || Callback == null) 
                    return;

                var retObj = JsonConvert.DeserializeObject(output, ReturnType);
                Callback.Invoke(retObj);
            }
        }

        static class ThreadManager
        {
            static Thread thread = null;
            static List<RequestObject> PendingRequests = new List<RequestObject>();

            public static void Start()
            {
                if (thread != null) return;

                thread = new Thread(() => RunThread());
            }

            public static void AddRequest(RequestObject newRequest)
            {
                PendingRequests.Add(newRequest);
            }

            static void RunThread()
            {
                while (thread.ThreadState != ThreadState.AbortRequested)
                {
                    while (PendingRequests.Count > 0)
                    {
                        var currentRequest = PendingRequests.First();
                        currentRequest.DoRequest();
                        PendingRequests.Remove(currentRequest);
                    }
                }
            }
        }

        private static Dictionary<string, string> Headers;
        private const string URLBase = "https://discordapp.com/api";

        public RESTHandler(string apiKey)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                { "Content-Type", "application/json" }
            };

            ThreadManager.Start();
        }

        public void DoRequest(string URL, string method, object data = null, Action<object> callback = null)
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