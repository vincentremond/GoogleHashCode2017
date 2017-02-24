using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public struct Cache
    {
        public int CacheId { get; set; }
        public int RemainingCapacity { get; set; }
        public IList<Video> Videos { get; set; }
        public IList<CacheToEndpointLatency> Endpoints { get; set; }
    }
}