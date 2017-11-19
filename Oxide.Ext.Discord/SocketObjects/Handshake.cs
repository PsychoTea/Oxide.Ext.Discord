namespace Oxide.Ext.Discord.DiscordObjects
{
    using Newtonsoft.Json;

    public class Handshake
    {
        public int op { get; set; }

        [JsonProperty("d")]
        public Payload Payload { get; set; }
    }
}
