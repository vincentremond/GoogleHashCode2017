namespace HashCode2017.DataObjects
{
    public struct Data
    {
        public Cache[] Caches { get; internal set; }
        public Endpoint[] Endpoints { get; internal set; }
        public Video[] Videos { get; internal set; }
    }
}
