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
                var a = -x.Value.CompareTo(y.Value);
                if (a != 0) return a;
                var b = x.Video.Size.CompareTo(y.Video.Size);
                if (b != 0) return b;
                var c = -x.Video.VideoId.CompareTo(y.Video.VideoId);
                if (c != 0) return c;
                var d = -x.Cache.CacheId.CompareTo(y.Cache.CacheId);
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
                SortPossibilities(ref possibilities,  comparer);
            }

            var initialPossibilitiesCount = possibilities.Count;
            var sortCount = 0;
            var startTime = DateTime.Now;
            var refTime = DateTime.Now;
            long? bestDisqualified = null;
            var disqualified = new List<Score>();
            using (new TimerArea("   Adding results... "))
            {
                Console.WriteLine();
                while (possibilities.Count > 0)
                {
                    if ((DateTime.Now - refTime).TotalSeconds > 60)
                    {
                        refTime = DateTime.Now;

                        var done = initialPossibilitiesCount - possibilities.Count;
                        var doneIn = (refTime - startTime).TotalSeconds;
                        var remainingDuration = TimeSpan.FromSeconds(possibilities.Count * doneIn / done);

                        Console.WriteLine($"{refTime:O} / Possibilities : {possibilities.Count} / Remaining time : {remainingDuration} / Sorts : {sortCount}");

                        sortCount = 0;
                    }

                    // need resort ?
                    if(bestDisqualified.HasValue && bestDisqualified.Value > possibilities[0].Value)
                    {
                        possibilities.AddRange(disqualified);
                        sortCount++;
                        bestDisqualified = null;
                        disqualified = new List<Score>();
                        SortPossibilities(ref possibilities, comparer);
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
                        if(!bestDisqualified.HasValue || bestDisqualified.Value > bestDisqualified.Value)
                        {
                            bestDisqualified = possibility.Value;
                        }
                        disqualified.Add(possibility);
                        continue;
                    }
                    cache.AddVideo(video);
                }
            }
        }

        private void SortPossibilities(ref List<Score> possibilities, ScoreComparer comparer)
        {
            possibilities.Sort(comparer);
            //possibilities = possibilities
            //    .AsParallel()
            //    .OrderBy(p => p.Value)
            //    .ThenByDescending(p => p.Video.Size)
            //    .ToList();
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
