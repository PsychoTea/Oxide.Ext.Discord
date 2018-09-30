namespace Oxide.Ext.Discord.Helpers
{
    using Oxide.Ext.Discord.DiscordObjects;
    using System;

    public class CreationTime
    {
        public static DateTime GetFromUser(User user) => GetFromUserID(user.id);

        public static DateTime GetFromUserID(string userID)
        {
            long id = long.Parse(userID);

            long ageInSeconds = ((id >> 22) + 1420070400000) / 1000;

            return new DateTime(1970, 1, 1).AddSeconds(ageInSeconds);
        }
    }
}
