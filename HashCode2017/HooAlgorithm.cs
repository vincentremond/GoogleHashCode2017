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

        public class Score
        {
            public Video Video { get; set; }
            public Cache Cache { get; set; }
            public long? Value { get; set; }
        }

        public void Calculate()
        {
            var possibilities = (
                    from v in _data.Videos
                    from c in _data.Caches
                    select new Score
                    {
                        Video = v,
                        Cache = c,
                        Value = null,
                    }
                ).ToList();

            var last = DateTime.Now;
            Console.WriteLine("Calculating scores");

            Score bestScore = null;
            do
            {
                if ((DateTime.Now - last).TotalSeconds >= 10)
                {
                    last = DateTime.Now;
                    Console.WriteLine(possibilities.Count);
                }

                CleanPossibilities(possibilities);
                ComputeScores(possibilities);
                possibilities.RemoveAll(p => !p.Value.HasValue || p.Value.Value == 0);
                bestScore = GetBestScore(possibilities);
                if (bestScore != null)
                {
                    // add best video to cache
                    bestScore.Cache.Videos.Add(bestScore.Video);
                    // reset score for impacted possibilities
                    foreach (var p in possibilities.Where(p => p.Video == bestScore.Video || p.Cache == bestScore.Cache))
                    {
                        p.Value = null;
                    }
                    possibilities.Remove(bestScore);
                }
            } while (bestScore != null);
        }

        protected void ComputeScores(List<Score> possibilities)
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

        protected Score GetBestScore(List<Score> possibilities)
        {
            return possibilities
                .OrderByDescending(p => p.Value.Value)
                .ThenBy(p => p.Video.Size)
                .FirstOrDefault();
        }

        protected void CleanPossibilities(List<Score> possibilities)
        {
            for (int index = possibilities.Count - 1; index >= 0; index--)
            {
                var possibility = possibilities[index];
                var video = possibility.Video;
                var cache = possibility.Cache;
                var remove = false;
                // video too big for cache ?
                if (cache.RemainingCapacity < video.Size)
                {
                    remove = true;
                }
                else if (cache.Endpoints.Sum(e => e.Endpoint.VideosRequest[video.VideoId]) == 0) // no gain on this video
                {
                    remove = true;
                }

                if (remove)
                {
                    possibilities.RemoveAt(index);
                }
            }
        }
    }
}
