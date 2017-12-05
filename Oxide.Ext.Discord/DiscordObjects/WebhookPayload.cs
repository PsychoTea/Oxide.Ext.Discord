namespace Oxide.Ext.Discord.DiscordObjects
{
    using System.Collections.Generic;
    using Oxide.Ext.Discord.DiscordObjects;

    public class WebhookPayload
    {
        public string content { get; set; }

        public string username { get; set; }

        public string avatar_url { get; set; }

        public bool tts { get; set; }

        public string file { get; set; }

        public List<Embed> embeds { get; set; }
    }
}
