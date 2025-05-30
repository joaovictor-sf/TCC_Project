namespace TCC_MVVM.Model
{
    public class ProcessLog
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public string WindowTitle { get; set; }
        public TimeSpan UsageTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public UserModel User { get; set; }
    }
}
