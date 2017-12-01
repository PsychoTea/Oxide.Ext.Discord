using System;
using System.Collections.Generic;

namespace Oxide.Ext.Discord.REST
{
    public class Bucket : List<Request>
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

        public void Fire(Request request, Action<RestResponse> callback) => request.Fire(callback);
    }
}
