using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oxide.Ext.Discord.REST
{
    class Bucket : List<Request>
    {
        public RequestMethod Method;
        public string Route;

        public int Limit;
        public int Remaining;
        public int Reset;

        public Bucket(RequestMethod method, string route)
        {
            this.Method = method;
            this.Route = route;
        }

        public Task<RestResponse> Fire(Request request)
        {
            while (request.Response == null) { }


        }
    }
}
