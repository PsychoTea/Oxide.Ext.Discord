using Newtonsoft.Json;
namespace Oxide.Ext.Discord.DiscordObjects
{
    class Handshake
    {
        [JsonProperty("op")]
        public int Op { get; set; }
        [JsonProperty("d")]
        public Payload payload { get; set; }
    }
}
