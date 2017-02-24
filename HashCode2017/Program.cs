using HashCode2017.DataObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;

namespace HashCode2017
{
    class Program
    {
        const string BasePath = @"C:\Users\vince\Downloads";
        static void Main(string[] args)
        {
            var files = new List<string>() { "sample" };
            foreach (var file in files)
            {
                var data = ReadInputFile(file);
                Calculate(data);
                WriteResult(file, data);
            }
        }

        private static void WriteResult(string inputFile, Data data)
        {
            var caches = data.Caches.Where(c => c.Videos.Count > 0).ToList();

            var result = new StringBuilder();

            result.Append(caches.Count).Append("\n");
            foreach (var cache in caches)
            {
                result.Append(cache.CacheId);
                foreach (var video in cache.Videos)
                {
                    result.Append(" ").Append(video.VideoId).Append("\n");
                }
            }

            var inputFilePath = Path.Combine(BasePath, $"{inputFile}.out");
            File.WriteAllText(inputFilePath, result.ToString());
        }

        public class Score
        {
            public int VideoId { get; set; }
            public int CacheId { get; set; }
            public int? Value { get; set; }
        }

        private static void Calculate(Data data)
        {
            var possibilities = (
                    from v in data.Videos
                    from c in data.Caches
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
                CleanPossibilities(data, possibilities);
                ComputeScores(possibilities);
                possibilities.RemoveAll(p => !p.Value.HasValue || p.Value.Value == 0);
                bestScore = GetBestScore(possibilities);
                if (bestScore != null)
                {
                    // add best video to cache
                    data.Caches[bestScore.CacheId].Videos.Add(data.Videos[bestScore.VideoId]);
                    // reset score for impacted possibilities
                    foreach (var p in possibilities.Where(p => p.VideoId == bestScore.VideoId || p.CacheId == bestScore.CacheId))
                    {
                        p.Value = null;
                    }
                }
            } while (bestScore != null);
        }

        private static void ComputeScores(List<Score> possibilities)
        {
            throw new NotImplementedException();
        }

        private static Score GetBestScore(List<Score> possibilities)
        {
            return possibilities.OrderBy(p => p.Value.Value).FirstOrDefault();
        }

        private static void CleanPossibilities(Data data, List<Score> possibilities)
        {
            for (int index = possibilities.Count - 1; index >= 0; index--)
            {
                var possibility = possibilities[index];
                var video = data.Videos[possibility.VideoId];
                var cache = data.Caches[possibility.CacheId];
                var remove = false;
                // video too big for cache ?
                if (cache.RemainingCapacity < video.Size)
                {
                    remove = true;
                }
                else if (cache.Endpoints.Sum(e => data.Endpoints[e.EndpointId].VideosRequest[video.VideoId]) == 0) // no gain on this video
                {
                    remove = true;
                }

                if (remove)
                {
                    possibilities.RemoveAt(index);
                }
            }
        }

        protected static Data ReadInputFile(string inputFile)
        {
            var data = new Data();

            // Read input file
            var inputFilePath = Path.Combine(BasePath, $"{inputFile}.in");
            var content = File.ReadAllLines(inputFilePath);
            var index = 0;
            var split = SplitInt(content[index++]);


            var videoCount = split[0];
            var endpointCount = split[1];
            var requestCount = split[2];
            var cacheCount = split[3];
            var cacheSize = split[4];

            data.Caches = GetInts(cacheCount)
                        .Select(id => new Cache
                        {
                            CacheId = id,
                            RemainingCapacity = cacheSize,
                            Endpoints = new List<CacheToEndpointLatency>(),
                            Videos = new List<Video>(),
                        })
                        .ToArray();

            data.Videos = SplitInt(content[index++])
                .Select((size, id) => new Video
                {
                    VideoId = id,
                    Size = size,
                })
                .ToArray();

            data.Endpoints = new Endpoint[endpointCount];
            for (int endpointIndex = 0; endpointIndex < endpointCount; endpointIndex++)
            {
                split = SplitInt(content[index++]);
                var endpoint = data.Endpoints[endpointIndex] = new Endpoint
                {
                    EndpointId = endpointIndex,
                    LatencyToDatacenter = split[0],
                    LinkedCacheCount = split[1],
                    VideosRequest = new int[videoCount],
                    Latencies = new List<EndpointLatencyToCache>(),
                };

                for (int linkedCacheIndex = 0; linkedCacheIndex < endpoint.LinkedCacheCount; linkedCacheIndex++)
                {
                    split = SplitInt(content[index++]);
                    endpoint.Latencies.Add(new EndpointLatencyToCache
                    {
                        CacheId = split[0],
                        LatencyToCache = split[1],
                    });
                }
            }

            for (int requestIndex = 0; requestIndex < requestCount; requestIndex++)
            {
                split = SplitInt(content[index++]);

                var videoId = split[0];
                var endpointId = split[1];
                var requestsCount = split[2];

                data.Endpoints[endpointId].VideosRequest[videoId] += requestCount;
            }

            // remapping caches to endpoints
            foreach (var endpoint in data.Endpoints)
            {
                foreach (var latency in endpoint.Latencies)
                {
                    data.Caches[latency.CacheId].Endpoints.Add(new CacheToEndpointLatency { EndpointId = endpoint.EndpointId, LatencyToEndpoint = latency.LatencyToCache });
                }
            }

            return data;
        }
        private static IEnumerable<int> GetInts(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return i;
            }
        }

        private static int[] SplitInt(string v)
        {
            return v.Split(' ').Select(x => int.Parse(x)).ToArray();
        }
    }
}
