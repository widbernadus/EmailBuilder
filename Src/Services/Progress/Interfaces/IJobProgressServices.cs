using Src.Services.Progress.Model;

namespace Src.Services.Progress.Interfaces
{
    public interface IJobProgressService
    {
        string Start(int total, string? jobId = null);

        void ReportSuccess();

        void ReportFailed();

        void ReportProcessed();

        void MarkCompleted();

        JobProgress GetProgress(string jobId);

        void Remove(int time = 0);
    }
}
