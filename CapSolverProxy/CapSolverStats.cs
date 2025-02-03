using Newtonsoft.Json;

namespace CapSolverProxy
{
    public class CapSolverStats
    {
        public readonly string serviceInfo;

        public int requests;
        public int fromcapsolver;
        public int fromcache;
        public int failed;
        public int errors;
        public int cached;
        public int cachecount;

        private readonly object _statsLock = new();

        public CapSolverStats()
        {
            serviceInfo = $"Service created at {DateTime.Now}";
        }

        public void IncRequests()
        {
            lock(_statsLock)
            {
                requests += 1;
            }
        }

        public void IncFromCapSolver()
        {
            lock(_statsLock)
            {
                fromcapsolver += 1;
            }
        }

        public void IncFromCache() { 
            lock(_statsLock)
            {
                fromcache += 1;
            }
        }

        public void IncFailed()
        {
            lock(_statsLock)
            { 
                failed += 1;
            }
        }
        
        public void IncErrors()
        {
            lock(_statsLock)
            {
                errors += 1;
            }
        }

        public void IncCached() 
        { 
            lock (_statsLock)
            {
                cached += 1;
            }
        }

        public string ToJson(int CurrentCacheCount)
        {
            string statsJson;
            lock (_statsLock)
            {
                cachecount = CurrentCacheCount;
                statsJson = JsonConvert.SerializeObject(this);
            }
            return statsJson;
        }
    }
}
