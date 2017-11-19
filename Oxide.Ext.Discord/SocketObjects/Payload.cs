namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Payload
    {
        public string token { get; set; }

        public PayloadProperty properties { get; set; }

        public bool compress { get; set; }

        public int large_threshold { get; set; }

        public int[] shard { get; set; } = new int[] { 0, 1, };
    }
}
