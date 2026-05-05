using Src.Services.Progress.Interfaces;
using Src.Services.Progress.Model;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Src.Services.Progress.Implementations
{
    public class JobProgressService: IJobProgressService
    {
        private readonly ConcurrentDictionary<string,JobProgressCounter> _jobs = new();
        private string _jobId;

        public string Start(int total, string? jobId = null)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                jobId = Guid.NewGuid().ToString();
            }   

            var counter = new JobProgressCounter(total);

            _jobs[jobId] = counter;
            _jobId = jobId;

            return _jobId;
        }

        public void ReportSuccess()
        {
            if (_jobs.TryGetValue(_jobId,
                out var job))
            {
                job.ReportSuccess();
            }
        }

        public void ReportFailed()
        {
            if (_jobs.TryGetValue(_jobId,
                out var job))
            {
                job.ReportFailed();
            }
        }

        public void ReportProcessed()
        {
            if (_jobs.TryGetValue(_jobId,
                out var job))
            {
                job.ReportProcessed();
            }
        }

        public void MarkCompleted()
        {
            if (_jobs.TryGetValue(_jobId,
                out var job))
            {
                job.MarkCompleted();
            }
        }

        public JobProgress GetProgress(string jobId)
        {
            if (_jobs.TryGetValue(jobId,
                out var job))
            {
                return job.GetProgress();
            }

            return new JobProgress();
        }

        /// <summary>
        /// Remove the job progress after a certain time (default is 10 seconds) to free up memory. 
        /// This is useful for long-running jobs where progress is no longer needed after completion.
        /// </summary>
        /// <param name="time"></param>
        public void Remove(int time = 10000)
        {
            Task.Delay(time).ContinueWith(_ =>
            {
                _jobs.TryRemove(_jobId, out var removed);
            });
        }
    }
}
