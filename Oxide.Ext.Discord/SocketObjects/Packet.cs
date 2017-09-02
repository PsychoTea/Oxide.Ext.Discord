using Newtonsoft.Json;

namespace Oxide.Ext.Discord.DiscordObjects
{
    class Packet
    {
        [JsonProperty("op", NullValueHandling = NullValueHandling.Ignore)]
        public int op { get; set; }
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
        public object d { get; set; }
        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int s { get; set; }
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string t { get; set; }
    }
}
