namespace Src.Services.Progress.Model
{
    public class JobProgress
    {
        public int Total { get; set; }

        public int Processed { get; set; }

        public int Success { get; set; }

        public int Failed { get; set; }

        public int Percent =>
            Total == 0
                ? 0
                : (int)((double)Processed / Total * 100);

        public bool IsCompleted { get; set; }
    }
}
