namespace Oxide.Ext.Discord.Gateway
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RPayload
    {
        [JsonProperty("op")]
        public OpCodes OpCode { get; set; }
        
        [JsonProperty("t")]
        public string EventName { get; set; }

        [JsonProperty("d")]
        public object _data { get; set; }

        [JsonProperty("s")]
        public int? Sequence { get; set; }

        public JObject EventData => _data as JObject;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }
    }
}
