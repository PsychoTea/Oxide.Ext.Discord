namespace Oxide.Ext.Discord.Exceptions
{
    using System;

    public class SettingsNullException : Exception
    {
        public SettingsNullException() : base("Error! Please supply a valid settings object!")
        {
        }
    }
}
