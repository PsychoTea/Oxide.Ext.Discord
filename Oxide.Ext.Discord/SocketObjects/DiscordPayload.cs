namespace Oxide.Ext.Discord.SocketObjects
{
    using Newtonsoft.Json;

    public class DiscordPayload
    {
        [JsonProperty("content")]
        public string MessageText { get; set; }
    }
}
