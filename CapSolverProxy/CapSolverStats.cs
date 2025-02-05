using Newtonsoft.Json;

namespace CapSolverProxy
{
    public class CapSolverStats
    {
        public readonly string serviceInfo;

        public CapSolverRequestsStats requests;
        public CapSolverSuccessStats success;
        public int failed;
        public int errors;
        public int cached;
        public int cacheCount;
        public CapSolverBalanceStats? lastBalance = null;

        private readonly object _statsLock = new();

        public CapSolverStats()
        {
            serviceInfo = $"Service created at {DateTime.Now}";
            requests = new CapSolverRequestsStats();
            success = new CapSolverSuccessStats();
        }

        public void IncCreateTaskRequests()
        {
            lock(_statsLock)
            {
                requests.createTask += 1;
            }
        }

        public void IncGetTaskResultRequests()
        {
            lock (_statsLock)
            {
                requests.getTaskResult += 1;
            }
        }

        public void IncSuccessFromCapSolver()
        {
            lock(_statsLock)
            {
                success.fromCapSolver += 1;
            }
        }

        public void IncSuccessFromCache() { 
            lock(_statsLock)
            {
                success.fromCache += 1;
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

        public void UpdateLastBalance(double balance)
        {
            lock (_statsLock)
            {
                if (lastBalance == null)
                {
                    lastBalance = new CapSolverBalanceStats();
                }
                lastBalance.balance = balance;
                lastBalance.balanceTime = $"{DateTime.Now}";
            }
        }

        public string ToJson(int CurrentCacheCount)
        {
            string statsJson;
            lock (_statsLock)
            {
                cacheCount = CurrentCacheCount;
                statsJson = JsonConvert.SerializeObject(this);
            }
            return statsJson;
        }
    }
}
