namespace Oxide.Ext.Discord.DiscordEvents
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Oxide.Ext.Discord.DiscordObjects;

    public class Identify
    {
        [JsonProperty("token")]
        public string Token;

        [JsonProperty("properties")]
        public Properties Properties;

        [JsonProperty("compress")]
        public bool Compress;

        [JsonProperty("large_threshold")]
        public int LargeThreshold;

        [JsonProperty("shard")]
        public List<int> Shard;

        [JsonProperty("presence")]
        public Presence Presence;
    }

    public class Properties
    {
        [JsonProperty("$os")]
        public string OS;

        [JsonProperty("$browser")]
        public string Browser;

        [JsonProperty("$device")]
        public string Device;
    }
}
