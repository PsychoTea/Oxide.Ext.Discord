namespace Oxide.Ext.Discord.Exceptions
{
    using System;

    public class APIKeyException : Exception
    {
        public APIKeyException() : base("Error! Please supply a valid API key!")
        {
        }
    }
}
