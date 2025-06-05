namespace TCC_MVVM.Model
{
    class DailyWorkLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeWorked { get; set; }

        public UserModel User { get; set; }
    }
}
