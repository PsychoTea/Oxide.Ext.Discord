namespace Oxide.Ext.Discord.Exceptions
{
    using System;

    public class InvalidCreationException : Exception
    {
        public InvalidCreationException() : base("Error! Please create your client using Discord.GetClient!")
        {
        }
    }
}
