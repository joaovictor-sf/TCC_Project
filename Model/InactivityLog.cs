using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_MVVM.Model
{
    /// <summary>
    /// Representa um log de inatividade de um usuário em um determinado dia.
    /// </summary>
    public class InactivityLog
    {
        /// <summary>
        /// Identificador único do log.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Data do registro (apenas data, sem hora).
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Tempo total em que o usuário esteve inativo no dia.
        /// </summary>
        public TimeSpan TotalInactivity { get; set; }

        /// <summary>
        /// Maior período contínuo de inatividade.
        /// </summary>
        public TimeSpan MaxInactivity { get; set; }

        /// <summary>
        /// Identificador do usuário associado ao log.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navegação para o usuário dono do log.
        /// </summary>
        public UserModel User { get; set; }
    }
}
