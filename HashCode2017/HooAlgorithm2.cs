using HashCode2017.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashCode2017
{
    public class HooAlgorithm2
    {
        protected class Score
        {
            public Video Video;
            public Cache Cache;
            public long Value;

            public override string ToString()
            {
                return $"[Video:{Video.VideoId}/Cache:{Cache.CacheId}/Gain:{Value}]";
            }
        }
        
        private Data _data;

        public HooAlgorithm2(Data data)
        {
            _data = data;
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
                        }
                    ).ToList();
            }

            using (new TimerArea("   Calculating scores... "))
            {
                ComputeScores(possibilities);
                possibilities.RemoveAll(p => p.Value <= 0);
            }
            
            var initialPossibilitiesCount = possibilities.Count;
            var startTime = DateTime.Now;
            var refTime = DateTime.Now;
            var disqualified = new List<Score>();
            using (new TimerArea("   Adding results... "))
            {
                Console.WriteLine();
                while (possibilities.Count > 0)
                {
                    if ((DateTime.Now - refTime).TotalSeconds > 2)
                    {
                        refTime = DateTime.Now;

                        var done = initialPossibilitiesCount - possibilities.Count;
                        var doneIn = (refTime - startTime).TotalSeconds;
                        var remainingDuration = TimeSpan.FromSeconds(possibilities.Count * doneIn / done);

                        Console.WriteLine($"{refTime:O} {possibilities.Count} {remainingDuration}");
                    }

                    var possibility = possibilities
                        .AsParallel()
                        .OrderByDescending(p => p.Value)
                        .ThenBy(p => p.Video.Size)
                        .First();
                    possibilities.Remove(possibility);

                    var video = possibility.Video;
                    var cache = possibility.Cache;

                    if (cache.RemainingCapacity < video.Size || possibility.Value <= 0)
                    {
                        continue;
                    }

                    cache.AddVideo(video);
                    UpdateContext(possibility, possibilities);
                }
            }
        }

        private void UpdateContext(Score addedPossibility, List<Score> possibilities)
        {
            foreach (var possibility in possibilities.Where(p=> p.Video == addedPossibility.Video))
            {
                possibility.Value = possibility.Cache.Endpoints.Sum(e =>
                {
                    var cacheLatency = e.Endpoint.Caches.Where(c => c.Cache.Videos.Contains(possibility.Video)).OrderBy(c => c.Latency).FirstOrDefault();
                    var latency = cacheLatency != null ? cacheLatency.Latency : e.Endpoint.LatencyToDatacenter;
                    var score = e.Endpoint.VideosRequest[possibility.Video.VideoId] * (latency - e.LatencyToEndpoint);
                    return score;
                });
            }
        }
        
        private void ComputeScores(List<Score> possibilities)
        {
            Parallel.ForEach(possibilities, possibility =>
            {
                possibility.Value = possibility.Cache.Endpoints.Sum(e => e.Endpoint.VideosRequest[possibility.Video.VideoId] * (e.Endpoint.LatencyToDatacenter - e.LatencyToEndpoint));
            });
        }
    }
}
