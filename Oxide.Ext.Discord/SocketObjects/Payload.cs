using Newtonsoft.Json;

namespace Oxide.Ext.Discord.DiscordObjects
{
    class Payload
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("properties")]
        public Payload_Property Property { get; set; }
        [JsonProperty("compress")]
        public bool Compress { get; set; }
        [JsonProperty("large_threshold")]
        public int Large_Threshold { get; set; }
        [JsonProperty("shard")]
        public int[] Shard { get; set; } = new int[] { 0, 1, };
    }
}
