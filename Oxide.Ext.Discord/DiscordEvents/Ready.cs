namespace Oxide.Ext.Discord.DiscordEvents
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Oxide.Ext.Discord.DiscordObjects;

    public class Ready
    {
        [JsonProperty("v")]
        public int Version { get; private set; }
        
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("private_channels")]
        public List<Channel> PrivateChannels { get; set; }
        
        [JsonProperty("guilds")]
        public List<Guild> Guilds { get; set; }

        [JsonProperty("session_id")]
        public string SessionID { get; set; }

        [JsonProperty("_trace")]
        public string[] Trace { get; set; }
    }
}
