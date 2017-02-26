using System;
using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Cache
    {
        public int CacheId;
        public int RemainingCapacity;
        public IList<Video> Videos;
        public IList<CacheToEndpointLatency> Endpoints;

        internal void AddVideo(Video video)
        {
            RemainingCapacity -= video.Size;
            Videos.Add(video);
        }
    }
}