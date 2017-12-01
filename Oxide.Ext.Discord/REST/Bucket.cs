namespace Oxide.Ext.Discord.REST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class Bucket : List<Request>
    {
        public RequestMethod Method;

        public string Route;

        public int Limit;

        public int Remaining;

        public int Reset;

        public bool Initialized = false;

        public bool Disposed = false;

        private Thread thread;

        public Bucket(RequestMethod method, string route)
        {
            this.Method = method;
            this.Route = route;

            thread = new Thread(() => RunThread());
            thread.Start();
        }

        public void Close()
        {
            thread?.Abort();
            thread = null;
        }

        public void Queue(Request request)
        {
            this.Add(request);

            if (!this.Initialized)
            {
                this.Initialized = true;
            }
        }

        private void RunThread()
        {
            // 'Initialized' basically allows us to start the while
            // loop from the constructor even when this.Count = 0
            // (eg after the bucket is created, before requests are added)
            while (!Initialized || (this.Count > 0))
            {
                if (Disposed) break;

                if (!Initialized) continue;

                FireRequests();
            }
        }

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
