using System;

namespace Oxide.Ext.Discord.Exceptions
{
    public class NoURLException : Exception
    {
        public NoURLException() : base("Error! No WSSURL was found.") { }
    }
}
