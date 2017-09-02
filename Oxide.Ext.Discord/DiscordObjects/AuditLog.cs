using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class AuditLog
    {
        public List<Webhook> webhooks { get; set; }
        public List<User> users { get; set; }
        public List<AuditLogEntry> audit_log_entries { get; set; }

        public static void GetGuildAuditLog(DiscordClient client, string guildID, Action<AuditLog> callback = null)
        {
            client.REST.DoRequest<AuditLog>($"/guilds/{guildID}/audit-logs", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as AuditLog);
            });
        }
    }
}
