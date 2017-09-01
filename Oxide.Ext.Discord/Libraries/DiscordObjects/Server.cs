using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Server
    {
        public List<object> voice_states { get; set; }
        public int verification_level { get; set; }
        public bool unavailable { get; set; }
        public object system_channel_id { get; set; }
        public object splash { get; set; }
        public List<Role> roles { get; set; }
        public string region { get; set; }
        public List<Presence> presences { get; set; }
        public string owner_id { get; set; }
        public string name { get; set; }
        public int mfa_level { get; set; }
        public List<Member> members { get; set; }
        public int member_count { get; set; }
        public bool large { get; set; }
        public string joined_at { get; set; }
        public string id { get; set; }
        public object icon { get; set; }
        public List<object> features { get; set; }
        public int explicit_content_filter { get; set; }
        public List<Emoji> emojis { get; set; }
        public int default_message_notifications { get; set; }
        public List<Channel> channels { get; set; }
        public object application_id { get; set; }
        public int afk_timeout { get; set; }
        public string afk_channel_id { get; set; }
    }
}
