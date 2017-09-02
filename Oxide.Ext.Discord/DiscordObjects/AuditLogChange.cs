namespace Oxide.Ext.Discord.DiscordObjects
{
    public class AuditLogChange
    {
        public AuditLogChangeKey new_value { get; set; }
        public AuditLogChangeKey old_value { get; set; }
        public string key { get; set; }
    }
}
