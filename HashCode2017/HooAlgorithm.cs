using HashCode2017.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashCode2017
{
    public class HooAlgorithm
    {
        private Data _data;

        public HooAlgorithm(Data data)
        {
            _data = data;
        }

        private class Score
        {
            public Video Video { get; set; }
            public Cache Cache { get; set; }
            public long? Value { get; set; }
        }

        public void Calculate()
        {
            List<Score> possibilities;
            using (new TimerArea("   Generating possibilities... "))
            {
                possibilities = (
                        from v in _data.Videos
                        from c in _data.Caches
                        select new Score
                        {
                            Video = v,
                            Cache = c,
                            Value = null,
                        }
                    ).ToList();
            }

            using (new TimerArea("   Calculating scores... "))
            {
                ComputeScores(possibilities);
                possibilities.RemoveAll(p => !p.Value.HasValue || p.Value.Value == 0);
            }

            using (new TimerArea("   Sorting... "))
            {
                possibilities = possibilities
                    .AsParallel()
                    .OrderByDescending(p => p.Value.Value)
                    .ThenBy(p => p.Video.Size)
                    .ToList();
            }

            using (new TimerArea("   Adding results... "))
            {
                foreach (var possibility in possibilities)
                {
                    var video = possibility.Video;
                    var cache = possibility.Cache;
                    if (cache.RemainingCapacity < video.Size)
                    {
                        continue;
                    }
                    cache.AddVideo(video);
                }
            }
        }

        private void ComputeScores(List<Score> possibilities)
        {
            Parallel.ForEach(possibilities, possibility =>
            {
                if (possibility.Value.HasValue)
                {
                    return;
                }

                possibility.Value = possibility.Cache.Endpoints.Sum(e => e.Endpoint.VideosRequest[possibility.Video.VideoId] * (e.Endpoint.LatencyToDatacenter - e.LatencyToEndpoint));
            });
        }
    }
}
