using System;

namespace Oxide.Ext.Discord.Libraries.Exceptions
{
    public class NoURLException : Exception
    {
        public NoURLException() : base("Error! No WSSURL was found.") { }
    }
}
