using HashCode2017.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;

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
            public int VideoId { get; set; }
            public int CacheId { get; set; }
            public int? Value { get; set; }
        }

        public void Calculate()
        {
            var possibilities = (
                    from v in _data.Videos
                    from c in _data.Caches
                    select new Score
                    {
                        VideoId = v.VideoId,
                        CacheId = c.CacheId,
                        Value = null,
                    }
                ).ToList();

            Score bestScore = null;
            do
            {
                CleanPossibilities(possibilities);
                ComputeScores(possibilities);
                possibilities.RemoveAll(p => !p.Value.HasValue || p.Value.Value == 0);
                bestScore = GetBestScore(possibilities);
                if (bestScore != null)
                {
                    // add best video to cache
                    _data.Caches[bestScore.CacheId].Videos.Add(_data.Videos[bestScore.VideoId]);
                    // reset score for impacted possibilities
                    foreach (var p in possibilities.Where(p => p.VideoId == bestScore.VideoId || p.CacheId == bestScore.CacheId))
                    {
                        p.Value = null;
                    }
                    possibilities.Remove(bestScore);
                }
            } while (bestScore != null);
        }

        protected void ComputeScores(List<Score> possibilities)
        {
            foreach (var possibility in possibilities)
            {
                if (possibility.Value.HasValue)
                {
                    continue;
                }

                var cache = _data.Caches[possibility.CacheId];
                var video = _data.Videos[possibility.VideoId];

                possibility.Value = cache.Endpoints.Sum(e => e.Endpoint.VideosRequest[video.VideoId] * (e.Endpoint.LatencyToDatacenter - e.LatencyToEndpoint));
            }
        }

        protected Score GetBestScore(List<Score> possibilities)
        {
            return possibilities.OrderByDescending(p => p.Value.Value).FirstOrDefault();
        }

        protected void CleanPossibilities(List<Score> possibilities)
        {
            for (int index = possibilities.Count - 1; index >= 0; index--)
            {
                var possibility = possibilities[index];
                var video = _data.Videos[possibility.VideoId];
                var cache = _data.Caches[possibility.CacheId];
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
