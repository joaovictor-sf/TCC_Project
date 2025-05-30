using System.ComponentModel.DataAnnotations;
using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Model
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        //[Required]
        //public string Role { get; set; }  // "Funcionario", "RH", "Admin"
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public WorkHours WorkHours { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<ProcessLog> ProcessLogs { get; set; }
        public ICollection<InactivityLog> InactivityLogs { get; set; }

        public UserModel() {
            ProcessLogs = new HashSet<ProcessLog>();
            InactivityLogs = new HashSet<InactivityLog>();
        }
    }
}
