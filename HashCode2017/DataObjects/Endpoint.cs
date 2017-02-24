using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Endpoint
    {
        public int Id { get; internal set; }
        public IList<EndpointLatencyToCache> Latencies { get; set; } = new List<EndpointLatencyToCache>();
        public int LatencyToDatacenter { get; internal set; }
        public int LinkedCacheCount { get; internal set; }
    }
}