using Newtonsoft.Json;
namespace Oxide.Ext.Discord.Libraries.SocketObjects
{
    public class DiscordPayload
    {
        [JsonProperty("content")]
        public string MessageText { get; set; }
    }
}
