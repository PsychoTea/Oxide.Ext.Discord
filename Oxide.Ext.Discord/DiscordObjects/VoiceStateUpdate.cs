namespace Oxide.Ext.Discord.DiscordObjects
{
    using Newtonsoft.Json;

    public class VoiceStateUpdate
    {
        [JsonProperty("guild_id")]
        public string GuildID;

        [JsonProperty("channel_id")]
        public string ChannelID;

        [JsonProperty("self_mute")]
        public bool SelfMute;

        [JsonProperty("self_deaf")]
        public bool SelfDeaf;
    }
}
