namespace Oxide.Ext.Discord.Helpers
{
    using System;

    public static class Time
    {
        public static double TimeSinceEpoch() => (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }
}
