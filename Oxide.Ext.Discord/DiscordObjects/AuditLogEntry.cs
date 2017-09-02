using System.Collections.Generic;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class AuditLogEntry
    {
        public string target_id { get; set; }
        public List<AuditLogChange> changes { get; set; }
        public string user_id { get; set; }
        public string id { get; set; }
        public int action_type { get; set; }
        public OptionalAuditEntryInfo options { get; set; }
        public string reason { get; set; }
    }
}
