using System;
using System.Collections.Generic;

namespace HashCode2017.DataObjects
{
    public class Cache
    {
        public int CacheId;
        public int RemainingCapacity;
        public IList<Video> Videos;
        public bool[] HasVideo;
        public IList<CacheToEndpointLatency> Endpoints;

        internal void AddVideo(Video video)
        {
            HasVideo[video.VideoId] = true;
            RemainingCapacity -= video.Size;
            Videos.Add(video);
        }
    }
}