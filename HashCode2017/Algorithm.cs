using HashCode2017.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashCode2017
{
    public class Algorithm
    {
        protected class Score
        {
            public Video Video;
            public Cache Cache;
            public long Value;

            public override string ToString()
            {
                return $"[Gain:{Value}/Video:{Video.VideoId}/Cache:{Cache.CacheId}]";
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
                return 0;
            }
        }

        private Data _data;

        public Algorithm(Data data)
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
                SortPossibilities(possibilities, comparer);
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
                while (possibilities.Count > 0 || RefillDisqualified(possibilities, disqualified, comparer))
                {
                    if ((DateTime.Now - refTime).TotalSeconds > 5)
                    {
                        refTime = DateTime.Now;

                        var done = initialPossibilitiesCount - possibilities.Count;
                        var doneIn = (refTime - startTime).TotalSeconds;
                        var remainingDuration = TimeSpan.FromSeconds(possibilities.Count * doneIn / done);

                        Console.WriteLine($"{refTime:O} / Poss : {possibilities.Count} / Rem : {remainingDuration} / Sorts : {sortCount} / Disc : {disqualified.Count}");

                        sortCount = 0;
                    }

                    // need resort ?
                    if (bestDisqualified.HasValue && bestDisqualified.Value > possibilities.Last().Value)
                    {
                        if (disqualified.Count > 0)
                        {
                            possibilities.AddRange(disqualified);
                            SortPossibilities(possibilities, comparer);
                        }
                        sortCount++;
                        bestDisqualified = null;
                        disqualified = new List<Score>();
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
                        if (!bestDisqualified.HasValue || possibility.Value > bestDisqualified.Value)
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

        private bool RefillDisqualified(List<Score> possibilities, List<Score> disqualified, ScoreComparer comparer)
        {
            if (disqualified.Count > 0)
            {
                possibilities.AddRange(disqualified);
                SortPossibilities(possibilities, comparer);
                disqualified.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SortPossibilities(List<Score> possibilities, ScoreComparer comparer)
        {
            possibilities.Sort(comparer);
        }

        private void RecalculateWithContext(Score possibility)
        {
            possibility.Value = possibility.Cache.Endpoints.Sum(e =>
            {
                var cacheLatency = e.Endpoint.Caches
                    .Where(c => c.Cache.HasVideo[possibility.Video.VideoId])
                    .OrderBy(c => c.Latency)
                    .FirstOrDefault();
                var newLatency = (cacheLatency != null ? cacheLatency.Latency : e.Endpoint.LatencyToDatacenter) - e.LatencyToEndpoint;
                if (newLatency < 0)
                {
                    return 0;
                }
                else
                {
                    return e.Endpoint.VideosRequest[possibility.Video.VideoId] * newLatency;
                }
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
