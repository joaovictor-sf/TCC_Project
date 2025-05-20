namespace TCC_MVVM.Model
{
    class InactivityLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TotalInactivity { get; set; }
        public TimeSpan MaxInactivity { get; set; }
    }
}
