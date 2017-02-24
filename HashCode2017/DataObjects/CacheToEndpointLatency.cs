namespace HashCode2017.DataObjects
{
    public struct CacheToEndpointLatency
    {
        public Endpoint Endpoint { get; internal set; }
        public int EndpointId { get; set; }
        public int LatencyToEndpoint { get; set; }
    }
}