using HashCode2017.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashCode2017
{
    public class HooAlgorithm
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

        protected class ScoreComparer : IComparer<Score>
        {
            public int Compare(Score x, Score y)
            {
                var a = x.Value.CompareTo(y.Value);
                if (a != 0) return a;
                var b = -x.Video.Size.CompareTo(y.Video.Size);
                if (b != 0) return b;
                var c = x.Video.VideoId.CompareTo(y.Video.VideoId);
                if (c != 0) return c;
                var d = x.Cache.CacheId.CompareTo(y.Cache.CacheId);
                if (d != 0) return d;
                //throw new InvalidOperationException("Duplicates ?");                
                return 0;
            }
        }

        private Data _data;

        public HooAlgorithm(Data data)
        {
            _data = data;
        }

        public void Calculate()
        {
            var comparer = new ScoreComparer();
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

            using (new TimerArea("   Sorting... "))
            {
                possibilities.Sort(comparer);
            }

            var refTime = DateTime.Now;

            using (new TimerArea("   Adding results... "))
            {
                Console.WriteLine();
                while (possibilities.Count > 0)
                {
                    if ((DateTime.Now - refTime).TotalMinutes > 2)
                    {
                        refTime = DateTime.Now;
                        Console.WriteLine($"{refTime:O} {possibilities.Count}");
                    }

                    var possibility = possibilities.GetAndRemoveLast();
                    var video = possibility.Video;
                    var cache = possibility.Cache;

                    if (cache.RemainingCapacity < video.Size || possibility.Value <= 0)
                    {
                        continue;
                    }

                    var oldScore = possibility.Value;
                    RecalculateWithContext(possibility);

                    if (possibility.Value <= 0)
                    {
                        continue;
                    }

                    if (oldScore != possibility.Value)
                    {
                        possibilities.AddSorted(possibility, comparer);
                        continue;
                    }
                    cache.AddVideo(video);
                }
            }
        }

        private void RecalculateWithContext(Score possibility)
        {
            possibility.Value = possibility.Cache.Endpoints.Sum(e =>
            {
                var cacheLatency = e.Endpoint.Caches.Where(c => c.Cache.Videos.Contains(possibility.Video)).OrderBy(c => c.Latency).FirstOrDefault();
                var latency = cacheLatency != null ? cacheLatency.Latency : e.Endpoint.LatencyToDatacenter;
                var score = e.Endpoint.VideosRequest[possibility.Video.VideoId] * (latency - e.LatencyToEndpoint);
                return score;
            });
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
