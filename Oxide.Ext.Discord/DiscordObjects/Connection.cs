using System.Collections.Generic;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Connection
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool? revoked { get; set; }
        public List<Integration> integrations { get; set; }
    }
}
