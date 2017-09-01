using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Server
    {
        public class Role
        {
            public int position { get; set; }
            public int permissions { get; set; }
            public string name { get; set; }
            public bool mentionable { get; set; }
            public bool managed { get; set; }
            public string id { get; set; }
            public bool hoist { get; set; }
            public int color { get; set; }
        }
        public class Game
        {
            public int type { get; set; }
            public string name { get; set; }
        }

        public class Presence
        {
            public User user { get; set; }
            public string status { get; set; }
            public Game game { get; set; }
        }

        public class Member
        {
            public User user { get; set; }
            public List<string> roles { get; set; }
            public string nick { get; set; }
            public bool mute { get; set; }
            public string joined_at { get; set; }
            public bool deaf { get; set; }
        }

        public class Emoji
        {
            public List<object> roles { get; set; }
            public bool require_colons { get; set; }
            public string name { get; set; }
            public bool managed { get; set; }
            public string id { get; set; }
        }

        public class Channel
        {
            public string type { get; set; }
            public object topic { get; set; }
            public int position { get; set; }
            public List<object> permission_overwrites { get; set; }
            public string name { get; set; }
            public string last_pin_timestamp { get; set; }
            public string last_message_id { get; set; }
            public bool is_private { get; set; }
            public string id { get; set; }
            public int? user_limit { get; set; }
            public object parent_id { get; set; }
            public bool? nsfw { get; set; }
            public int? bitrate { get; set; }
        }
        public Member FindMember(string id) => members.Find(x => x.user.id == id);
        public Channel FindChannel(string id) => channels.Find(x => x.id == id);
        public Emoji FindEmoji(string id) => emojis.Find(x => x.id == id);
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
