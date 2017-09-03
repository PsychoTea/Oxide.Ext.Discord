using System;

namespace Oxide.Ext.Discord.Exceptions
{
    public class PluginNullException : Exception
    {
        public PluginNullException() : base("Error! Please supply a valid plugin object!") { }
    }
}
