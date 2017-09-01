using Oxide.Core;
using Oxide.Ext.Discord.Libraries.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Message
    {
        public string webhook_id { get; set; }
        public int type { get; set; }
        public bool tts { get; set; }
        public string timestamp { get; set; }
        public bool pinned { get; set; }
        public object nonce { get; set; }
        public List<User> mentions { get; set; }
        public List<Role> mention_roles { get; set; }
        public bool mention_everyone { get; set; }
        public string id { get; set; }
        public List<Embed> embeds { get; set; }
        public Embed embed { get; set; }
        public object edited_timestamp { get; set; }
        public string content { get; set; }
        public string channel_id { get; set; }
        public Author author { get; set; }
        public List<object> attachments { get; set; }
    }
}
