using Newtonsoft.Json;
namespace Oxide.Ext.Discord.SocketObjects
{
    public class DiscordPayload
    {
        [JsonProperty("content")]
        public string MessageText { get; set; }
    }
}
