using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HiddenGems.Common
{
    public static class JobRequestManager
    {
        private static ConcurrentDictionary<string, KeyValuePair<DateTime, JobRequest>> _jobQueue = new();

        public static void ResetQueue()
        {
            _jobQueue = new();
        }

        public static JobRequest GetOrAddJobRequest(string jobKey)
        {
            PurgeJobs();
            var JobRequest = _jobQueue.GetOrAdd(jobKey,
                (x) => new KeyValuePair<DateTime, JobRequest>(DateTime.Now, new JobRequest(jobKey))).Value;
            JobRequest.JobKey = jobKey;
            return JobRequest;
        }

        public static JobRequest GetExistingJobRequest(string jobKey)
        {
            PurgeJobs();
            if (!string.IsNullOrWhiteSpace(jobKey) && _jobQueue.TryGetValue(jobKey, out var existingJobRequest))
            {
                return existingJobRequest.Value;
            }
            else
            {
                return null;
            }
        }

        public static int GetCurrentJobCount()
        {
            return _jobQueue.Count;
        }

        public static void PurgeJobs()
        {
            var expiredJobs = _jobQueue.Where(x => x.Value.Value.Status.CanRemove
            || x.Value.Key < DateTime.Now.AddMinutes(-SharedSettings.IdleTimeout))
                    .Select(y => y.Key)
                    .ToList();
            expiredJobs.ForEach(x => _jobQueue.Remove(x, out _));
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
