using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core;

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

            var response = req.GetResponse() as HttpWebResponse;
            var reader = new StreamReader(response.GetResponseStream());
            string output = reader.ReadToEnd().Trim();

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                Interface.Oxide.LogWarning($"An error occured whilst submitting a request to {req.RequestUri} (code {response.StatusCode}): {output}");
                return default(T);
            }
            
            return (T)JsonConvert.DeserializeObject(output, typeof(T));
        }
    }
}