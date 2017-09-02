namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Integration
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool enabled { get; set; }
        public bool syncing { get; set; }
        public string role_id { get; set; }
        public int expire_behaviour { get; set; }
        public int expire_grace_peroid { get; set; }
        public User user { get; set; }
        public Account account { get; set; }
        public string synced_at { get; set; }
    }
}
