using System;
using System.Linq;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord.Exceptions
{
    public class SocketRunningException : Exception
    {
        public SocketRunningException(DiscordClient client) : base($"Error! Tried to create a socket when one is already running @ {client.GetPluginNames()}") { }
    }
}
