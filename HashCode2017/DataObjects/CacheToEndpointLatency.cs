namespace HashCode2017.DataObjects
{
    public class CacheToEndpointLatency
    {
        public Endpoint Endpoint { get; internal set; }
        public int EndpointId { get; set; }
        public long LatencyToEndpoint { get; set; }
    }
}