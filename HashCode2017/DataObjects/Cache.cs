using System;
using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Cache
    {
        public int CacheId { get; set; }
        public int RemainingCapacity { get; set; }
        public IList<Video> Videos { get; set; }
        public IList<CacheToEndpointLatency> Endpoints { get; set; }

        internal void AddVideo(Video video)
        {
            RemainingCapacity -= video.Size;
            Videos.Add(video);
        }
    }
}