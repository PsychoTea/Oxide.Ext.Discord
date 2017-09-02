using Oxide.Ext.Discord.Libraries.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class User
    {
        public string username { get; set; }
        public string id { get; set; }
        public string discriminator { get; set; }
        public string avatar { get; set; }
        public bool? bot { get; set; }

        public static void GetUser(DiscordClient client, string userID, Action<User> callback = null)
        {
            client.REST.DoRequest<User>($"/users/{userID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as User);
            });
        }
    }
}
