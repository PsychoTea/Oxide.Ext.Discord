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
        public class Thumbnail
        {
            public int width { get; set; }
            public string url { get; set; }
            public string proxy_url { get; set; }
            public int height { get; set; }
        }

        public class Provider
        {
            public string url { get; set; }
            public string name { get; set; }
        }

        public class Embed
        {
            public string url { get; set; }
            public string type { get; set; }
            public string title { get; set; }
            public Thumbnail thumbnail { get; set; }
            public Provider provider { get; set; }
            public string description { get; set; }
            public Author author { get; set; }
        }

        public class Author
        {
            public string username { get; set; }
            public string id { get; set; }
            public string discriminator { get; set; }
            public bool bot { get; set; }
            public string avatar { get; set; }
        }
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
