using Oxide.Ext.Discord.Libraries.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Guild
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string splash { get; set; }
        public string owner_id { get; set; }
        public string region { get; set; }
        public string afk_channel_id { get; set; }
        public int afk_timeout { get; set; }
        public bool embed_enabled { get; set; }
        public string embed_channel_id { get; set; }
        public int verification_level { get; set; }
        public int default_message_notifications { get; set; }
        public int explicit_content_filter { get; set; }
        public List<Role> roles { get; set; }
        public List<Emoji> emojis { get; set; }
        public List<string> features { get; set; }
        public int mfa_level { get; set; }
        public string application_id { get; set; }
        public bool widget_enabled { get; set; }
        public string widget_channel_id { get; set; }
        public string joined_at { get; set; }
        public bool large { get; set; }
        public bool unavailable { get; set; }
        public int member_count { get; set; }
        public List<VoiceState> voice_states { get; set; }
        public List<Member> members { get; set; }
        public List<Channel> channels { get; set; }
        public List<Presence> presences { get; set; }

        [Obsolete("TODO: Add this.")]
        public void CreateGuild() { }
        
        public static void GetGuild(DiscordClient client, string guildID, Action<Guild> callback = null)
        {
            var guild = client.REST.DoRequest<Guild>($"/guilds/{guildID}", "GET");
            callback?.Invoke(guild);
        }

        [Obsolete("TODO: Add this.")]
        public void ModifyGuild() { }

        [Obsolete("TODO: Add this.")]
        public void DeleteGuild() { }

        public void GetGuildChannels(DiscordClient client, Action<List<Channel>> callback = null)
        {
            var channels = client.REST.DoRequest<List<Channel>>($"/guilds/{id}/channels", "GET");
            callback?.Invoke(channels);
        }
    }
}
