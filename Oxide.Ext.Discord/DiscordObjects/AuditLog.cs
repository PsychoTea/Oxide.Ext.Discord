namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using Oxide.Ext.Discord.REST;

    public class AuditLog
    {
        public List<Webhook> webhooks { get; set; }

        public List<User> users { get; set; }

        public List<AuditLogEntry> audit_log_entries { get; set; }

        public static void GetGuildAuditLog(DiscordClient client, Guild guild, Action<AuditLog> callback = null) => GetGuildAuditLog(client, guild.id, callback);

        public static void GetGuildAuditLog(DiscordClient client, string guildID, Action<AuditLog> callback = null)
        {
            client.REST.DoRequest($"/guilds/{guildID}/audit-logs", RequestMethod.GET, null, callback);
        }
    }
}
