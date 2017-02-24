using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Endpoint
    {
        public int EndpointId { get; internal set; }
        public IList<EndpointLatencyToCache> Latencies { get; set; }
        public long LatencyToDatacenter { get; internal set; }
        public int LinkedCacheCount { get; internal set; }
        public int[] VideosRequest { get; internal set; }
    }
}