using System.ComponentModel.DataAnnotations;
using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Model
{
    /// <summary>
    /// Representa um usuário do sistema com informações de login, papel e configuração de jornada de trabalho.
    /// </summary>
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome de usuário usado para login.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Hash da senha criptografada do usuário.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Primeiro nome do usuário.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sobrenome do usuário.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Papel do usuário no sistema (ADMIN, RH ou DEV).
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Carga horária de trabalho configurada para o usuário.
        /// </summary>
        public WorkHours WorkHours { get; set; }

        /// <summary>
        /// Indica se o usuário está ativo no sistema.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Logs de uso de aplicações registrados pelo usuário.
        /// </summary>
        public ICollection<ProcessLog> ProcessLogs { get; set; }

        /// <summary>
        /// Logs de inatividade registrados pelo usuário.
        /// </summary>
        public ICollection<InactivityLog> InactivityLogs { get; set; }

        /// <summary>
        /// Indica se o usuário deve trocar sua senha no próximo login.
        /// </summary>
        public bool MustChangePassword { get; set; } = false;

        public UserModel() {
            ProcessLogs = new HashSet<ProcessLog>();
            InactivityLogs = new HashSet<InactivityLog>();
        }
    }
}
