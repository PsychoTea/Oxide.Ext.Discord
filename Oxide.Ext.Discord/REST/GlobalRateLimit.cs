using System.Timers;

namespace Oxide.Ext.Discord.REST
{
    class GlobalRateLimit
    {
        public static bool Hit { get; private set; }
        
        private static Timer Timer;

        public static void Reached(int resetTime)
        {
            Hit = true;

            Timer = new Timer(resetTime)
            {
                Enabled = true
            };

            Timer.Elapsed += (s, e) =>
            {
                Hit = false;
            };
        }
    }
}
