namespace Oxide.Ext.Discord.DiscordObjects
{
    using Newtonsoft.Json;

    public class Packet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int op { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object d { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int s { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string t { get; set; }
    }
}
