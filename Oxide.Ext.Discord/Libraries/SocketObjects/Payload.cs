using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    class Payload
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("properties")]
        public Payload_Property Property { get; set; }
        [JsonProperty("compress")]
        public bool Compress;
        [JsonProperty("large_threshold")]
        public int Large_Threshold { get; set; }
        [JsonProperty("shard")]
        public int[] Shard = new List<int>() { 0, 1 }.ToArray();
    }
}
