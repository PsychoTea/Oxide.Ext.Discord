namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Role
    {
        public string id { get; set; }
        public string name { get; set; }
        public int? color { get; set; }
        public bool? hoist { get; set; }
        public int? position { get; set; }
        public int? permissions { get; set; }
        public bool? managed { get; set; }
        public bool? mentionable { get; set; }
    }
}
