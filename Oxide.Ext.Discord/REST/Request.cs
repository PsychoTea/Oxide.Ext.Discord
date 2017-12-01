using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;

namespace Oxide.Ext.Discord.REST
{
    public class Request
    {
        private const string URLBase = "https://discordapp.com/api";

        public RequestMethod Method { get; }
        public string Route { get; }
        public string Endpoint { get; }
        public Dictionary<string, string> Headers { get; }
        public object Data { get; }
        public RestResponse Response { get; }

        public Request(RequestMethod method, string route, string endpoint, Dictionary<string, string> headers, object data)
        {
            this.Method = method;
            this.Route = route;
            this.Endpoint = endpoint;
            this.Headers = headers;
            this.Data = data;
        }

        public void Fire(Action<RestResponse> callback)
        {
            string url = URLBase + Route;

            Interface.Oxide.LogInfo($"Request.Fire was called for {url}");

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

            callback.Invoke(new RestResponse(output));
        }
    }
}
