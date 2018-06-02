namespace Oxide.Ext.Discord.DiscordObjects
{
    using Newtonsoft.Json;

    public class Resume
    {
        [JsonProperty("token")]
        public string Token;

        [JsonProperty("session_id")]
        public string SessionID;

        [JsonProperty("sequence")]
        public int Sequence;
    }
}
