using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_MVVM.Model
{
    /// <summary>
    /// Representa um log de uso de uma aplicação por um usuário em um determinado dia.
    /// </summary>
    public class ProcessLog
    {
        /// <summary>
        /// Identificador único do log.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do processo da aplicação (ex: chrome, word).
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Título da janela ativa da aplicação.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Tempo total de uso da aplicação no dia.
        /// </summary>
        public TimeSpan UsageTime { get; set; }

        /// <summary>
        /// Data do registro (apenas data, sem hora).
        /// </summary>
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

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
