using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.Discord.REST
{
    class Request
    {
        public RequestMethod Method { get; }
        public string Route { get; }
        public string Endpoint { get; }
        public RestResponse Response { get; }

        public Request(RequestMethod method, string route, string endpoint)
        {
            this.Method = method;
            this.Route = route;
            this.Endpoint = endpoint;
        }
    }
}
