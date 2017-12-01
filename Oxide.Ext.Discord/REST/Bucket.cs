using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Oxide.Ext.Discord.REST
{
    public class Bucket : List<Request>
    {
        public RequestMethod Method;
        public string Route;

        public int Limit;
        public int Remaining;
        public int Reset;

        private Timer timer;

        public Bucket(RequestMethod method, string route)
        {
            this.Method = method;
            this.Route = route;

            timer = new Timer(250)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += Timer_Elapsed;
        }
        
        public void Queue(Request request) => this.Add(request);

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.Count == 0)
            {
                // Dispose
                timer.Dispose();
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
