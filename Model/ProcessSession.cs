namespace TCC_MVVM.Model
{
    class ProcessSession
    {
        public string Key { get; set; }
        public string AppName { get; set; }
        public string WindowTitle { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration {
            get {
                var end = EndTime ?? DateTime.UtcNow;
                return end - StartTime;
            }
        }
    }
}
