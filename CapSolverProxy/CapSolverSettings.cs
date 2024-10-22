namespace CapSolverProxy
{
    public class CapSolverSettings
    {
        public int CacheSizeLimit { get; set; }
        public int CacheSlidingExpiration { get; set; }
        public int CacheAbsoluteExpiration { get; set; }
        public string? ImagesFolder { get; set; }
        public bool SaveImages { get; set; }
    }
}
