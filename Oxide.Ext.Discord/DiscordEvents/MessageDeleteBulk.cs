namespace Oxide.Ext.Discord.DiscordEvents
{
    using System.Collections.Generic;

    public class MessageDeleteBulk
    {
        public List<string> ids { get; set; }

        public string channel_id { get; set; }
    }
}
