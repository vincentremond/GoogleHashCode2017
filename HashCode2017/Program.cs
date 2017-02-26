using HashCode2017.DataObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using System.Diagnostics;

namespace HashCode2017
{
    class Program
    {
        const string BasePath = @"C:\Users\vince\Downloads";
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;


            var files = new List<string>() { "sample", "me_at_the_zoo", "videos_worth_spreading", "trending_today", "kittens" };
            foreach (var file in files)
            {
                Console.WriteLine($"File: {file}");
                var sw = Stopwatch.StartNew();

                var data = ReadInputFile(file);
                var algo = new HooAlgorithm(data);
                algo.Calculate();
                WriteResult(file, data);

                sw.Stop();
                Console.Write($" {sw.Elapsed}");
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.ReadLine();
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
                    result.Append(" ").Append(video.VideoId);
                }
                result.Append("\n");
            }

            var inputFilePath = Path.Combine(BasePath, $"{inputFile}.out");
            File.WriteAllText(inputFilePath, result.ToString());
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
                    Caches = new List<LinkToCache>(),
                };

                for (int linkedCacheIndex = 0; linkedCacheIndex < endpoint.LinkedCacheCount; linkedCacheIndex++)
                {
                    split = SplitInt(content[index++]);
                    endpoint.Caches.Add(new LinkToCache
                    {
                        Cache = data.Caches[split[0]],
                        Latency = split[1],
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
                foreach (var latency in endpoint.Caches)
                {
                    latency.Cache.Endpoints.Add(new CacheToEndpointLatency { EndpointId = endpoint.EndpointId, Endpoint = endpoint, LatencyToEndpoint = latency.Latency });
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
