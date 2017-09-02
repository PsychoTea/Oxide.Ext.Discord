using System;
namespace Oxide.Ext.Discord.Exceptions
{
    public class InvalidCreationException : Exception
    {
        public InvalidCreationException() : base("Error! Please create your client using Discord.GetClient!") { }
    }
}
