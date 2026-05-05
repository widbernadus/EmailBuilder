using Src.Services.Progress.Model;
using System.Threading;

namespace Src.Services.Progress.Implementations
{
    public class JobProgressCounter
    {
        private int _total;
        private int _processed;
        private int _success;
        private int _failed;
        private int _completed;

        public JobProgressCounter(int total)
        {
            _total = total;
        }

        public void ReportSuccess()
        {
            Interlocked.Increment(ref _processed);
            Interlocked.Increment(ref _success);
        }

        public void ReportFailed()
        {
            Interlocked.Increment(ref _processed);
            Interlocked.Increment(ref _failed);
        }

        public void ReportProcessed()
        {
            Interlocked.Increment(ref _processed);
        }

        public void MarkCompleted()
        {
            Interlocked.Exchange(ref _completed, 1);
        }

        public JobProgress GetProgress()
        {
            var total =
                Volatile.Read(ref _total);

            var processed =
                Volatile.Read(ref _processed);

            var success =
                Volatile.Read(ref _success);

            var failed =
                Volatile.Read(ref _failed);

            return new JobProgress
            {
                Total = total,
                Processed = processed,
                Success = success,
                Failed = failed,
                IsCompleted =
                    Volatile.Read(ref _completed) == 1
            };
        }
    }
}
