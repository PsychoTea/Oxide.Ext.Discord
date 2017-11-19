namespace Oxide.Ext.Discord.Exceptions
{
    using System;

    public class LimitedClientException : Exception
    {
        public LimitedClientException() : base("Error! You may only use one apikey/client per plugin!")
        {
        }
    }
}
