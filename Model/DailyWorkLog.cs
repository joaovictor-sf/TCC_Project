using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_MVVM.Model
{
    /// <summary>
    /// Representa o total de horas trabalhadas por um usuário em um determinado dia.
    /// </summary>
    class DailyWorkLog {
        /// <summary>
        /// Identificador único do log diário.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o log pertence.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Data do registro (apenas a parte da data é considerada).
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Tempo total de trabalho registrado no dia.
        /// </summary>
        public TimeSpan TimeWorked { get; set; }

        /// <summary>
        /// Navegação para o usuário associado ao log.
        /// </summary>
        public UserModel User { get; set; }
    }
}
