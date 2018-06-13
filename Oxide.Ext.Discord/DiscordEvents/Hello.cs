namespace Oxide.Ext.Discord.DiscordEvents
{
    using Newtonsoft.Json;

    public class Hello
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval;

        [JsonProperty("_trace")]
        public string[] Trace;
    }
}
