using HashCode2017.DataObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashCode2017
{
    class Program
    {
        static void Main(string[] args)
        {
            var basePath = @"C:\Users\vince\Downloads";
            var inputFiles = new List<string>() { "sample" };

            foreach (var inputFile in inputFiles)
            {
                int videoCount, endpointCount, requestCount, cacheCount, cacheSize;

                // Read input file
                var inputFilePath = Path.Combine(basePath, $"{inputFile}.in");
                var content = File.ReadAllLines(inputFilePath);
                var index = 0;
                var split = SplitInt(content[index++]);
                videoCount = split[0];
                endpointCount = split[1];
                requestCount = split[2];
                cacheCount = split[3];
                cacheSize = split[4];

                var caches = GetInts(cacheCount)
                    .Select(id => new Cache
                    {
                        CacheId = id,
                        RemainingCapacity = cacheSize,
                    })
                    .ToList();

                var videos = SplitInt(content[index++])
                    .Select((size, id) => new Video
                    {
                        VideoId = id,
                        Size = size,
                    })
                    .ToList();

                var endpoints = new List<Endpoint>(endpointCount);
                for (int endpointIndex = 0; endpointIndex < endpointCount; endpointIndex++)
                {
                    split = SplitInt(content[index++]);
                    var endpoint = new Endpoint
                    {
                        Id = endpointIndex,
                        LatencyToDatacenter = split[0],
                        LinkedCacheCount = split[1],
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

                var requests = new List<Request>(requestCount);
                for (int requestIndex = 0; requestIndex < requestCount; requestIndex++)
                {
                    split = SplitInt(content[index++]);
                    requests.Add(new Request
                    {
                        VideoId = split[0],
                        EndpointId = split[1],
                        RequestsCount = split[2],
                    });
                }
            }
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
