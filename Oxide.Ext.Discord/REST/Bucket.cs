namespace Oxide.Ext.Discord.REST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;

    public class Bucket : List<Request>
    {
        public RequestMethod Method;

        public string Route;

        public int Limit;

        public int Remaining;

        public int Reset;

        public bool Disposed;
        
        public Bucket(RequestMethod method, string route)
        {
            this.Method = method;
            this.Route = route;

            while (this.Count > 0 && !Disposed)
            {
                FireRequests();
            }
        }
        
        public void Queue(Request request) => this.Add(request);

        private void FireRequests()
        {
            if (GlobalRateLimit.Hit)
            {
                return;
            }

            if (Remaining == 0 && Reset > TimeSinceEpoch())
            {
                return;
            }

            if (this.Any(x => x.InProgress))
            {
                return;
            }

            var nextItem = this.First();
            nextItem.Fire(this);
        }

        private double TimeSinceEpoch() => (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
