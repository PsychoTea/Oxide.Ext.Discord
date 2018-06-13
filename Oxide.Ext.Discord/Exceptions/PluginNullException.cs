namespace Oxide.Ext.Discord.Exceptions
{
    using System;

    public class PluginNullException : Exception
    {
        public PluginNullException() : base("Error! Please supply a valid plugin object!")
        {
        }
    }
}
