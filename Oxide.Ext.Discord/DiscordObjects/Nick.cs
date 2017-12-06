namespace Oxide.Ext.Discord.DiscordObjects
{
    using System.Collections.Generic;

    public class Nick
    {
        public string id { get; set; }

        public string nick { get; set; }
     
        public static implicit operator KeyValuePair<string, string>(Nick nick) => new KeyValuePair<string, string>(nick.id, nick.nick);
    }
}
