using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Endpoint
    {
        public int EndpointId;
        public IList<LinkToCache> Caches;
        public long LatencyToDatacenter;
        public int LinkedCacheCount;
        public int[] VideosRequest;
    }
}