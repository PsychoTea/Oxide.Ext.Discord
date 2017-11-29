using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oxide.Ext.Discord.REST
{
    class RESTHandler
    {
        public List<Bucket> Buckets = new List<Bucket>();

        public Task<RestResponse> CreateRequest(RequestMethod method, string route, string endpoint)
        {
            var request = new Request(method, route, endpoint);
            return BucketRequest(request);
        }

        private Task<RestResponse> BucketRequest(Request request)
        {
            var bucket = Buckets.SingleOrDefault(x => x.Method == request.Method &&
                                                      x.Route == request.Route);

            if (bucket != null)
            {
                bucket.Add(request);
                return bucket.Fire(request);
            }

            var newBucket = new Bucket(request.Method, request.Route);
            Buckets.Add(newBucket);

            newBucket.Add(request);
            return newBucket.Fire(request);
        }
    }
}
