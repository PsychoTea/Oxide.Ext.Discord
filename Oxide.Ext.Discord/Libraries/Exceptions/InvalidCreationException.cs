using System;
namespace Oxide.Ext.Discord.Libraries.Exceptions
{
    public class InvalidCreationException : Exception
    {
        public InvalidCreationException() : base("Error! Please create your client using the static method in the Discord class!") { }
    }
}
