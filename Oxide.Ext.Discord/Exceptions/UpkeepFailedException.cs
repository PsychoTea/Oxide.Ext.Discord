using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Exceptions
{
    public class UpkeepFailedException : Exception
    {
        public UpkeepFailedException(string apiKey) : base($"Api Key: {apiKey} has failed to keep up the connection with the extension!") { }
    }
}
