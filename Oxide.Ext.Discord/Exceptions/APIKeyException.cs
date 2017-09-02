using System;

namespace Oxide.Ext.Discord.Exceptions
{
    public class APIKeyException : Exception
    {
        public APIKeyException() : base("Error! Please supply a valid API key!") { }
    }
}
