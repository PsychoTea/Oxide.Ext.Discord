using System;

namespace Oxide.Ext.Discord.Libraries.Exceptions
{
    public class APIKeyException : Exception
    {
        public APIKeyException() : base("Error! Please supply a valid API key!") { }
    }
}
