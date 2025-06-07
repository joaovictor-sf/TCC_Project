using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_MVVM.Model
{
    public class InactivityLog
    {
        public int Id { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public TimeSpan TotalInactivity { get; set; }
        public TimeSpan MaxInactivity { get; set; }
        public int UserId { get; set; }
        public UserModel User { get; set; }
    }
}
