using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core.Libraries;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class RESTHandler
    {
        private Dictionary<string, string> Headers;
        private const string URLBase = "https://discordapp.com/api";

        public RESTHandler(string apiKey)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Authorization", $"Bot {apiKey}" },
                { "Content-Type", "application/json" }
            };
        }

        public void DoRequest(string URL, string method, object data = null)
        {
            var req = WebRequest.Create($"{URLBase}{URL}");
            req.SetRawHeaders(Headers);
            req.Method = method;

            if (data != null)
            {
                string contents = JsonConvert.SerializeObject(data);
                Core.Interface.Oxide.LogInfo(contents);
                byte[] bytes = Encoding.ASCII.GetBytes(contents);
                req.ContentLength = bytes.Length;

                using (var stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            req.GetResponse();
        }

        public T DoRequest<T>(string URL, string method, object data = null)
        {
            var req = WebRequest.Create($"{URLBase}{URL}");
            req.SetRawHeaders(Headers);
            req.Method = method;

            if (data != null)
            {
                string contents = JsonConvert.SerializeObject(data);
                byte[] bytes = Encoding.ASCII.GetBytes(contents);
                req.ContentLength = bytes.Length;

                using (var stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            var response = req.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return (T)JsonConvert.DeserializeObject(reader.ReadToEnd().Trim(), typeof(T));
        }
    }
}