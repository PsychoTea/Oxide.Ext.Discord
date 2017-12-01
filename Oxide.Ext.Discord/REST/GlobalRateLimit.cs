namespace Oxide.Ext.Discord.REST
{
    using System.Timers;

    public class GlobalRateLimit
    {
        public static bool Hit { get; private set; } = false;
        
        private static Timer timer;

        public static void Reached(int resetTime)
        {
            Hit = true;

            timer = new Timer(resetTime)
            {
                Enabled = true
            };

            timer.Elapsed += (s, e) =>
            {
                Hit = false;
            };
        }
    }
}
