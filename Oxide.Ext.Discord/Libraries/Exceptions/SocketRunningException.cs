using System;

namespace Oxide.Ext.Discord.Libraries.Exceptions
{
    public class SocketRunningException : Exception
    {
        public SocketRunningException() : base("Error! Tried to create a socket when one is already running.") { }
    }
}
