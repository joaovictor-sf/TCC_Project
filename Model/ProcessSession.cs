namespace TCC_MVVM.Model
{
    /// <summary>
    /// Representa uma sessão de execução de um processo monitorado no sistema.
    /// </summary>
    class ProcessSession {
        /// <summary>
        /// Chave única composta por nome do processo e título da janela.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Nome do processo da aplicação.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Título da janela ativa do processo.
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// Data e hora em que a sessão do processo foi iniciada.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora em que a sessão do processo foi encerrada. 
        /// Se estiver nulo, o processo ainda está ativo.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Duração da sessão de uso do processo.
        /// Se ainda não finalizado, calcula com base na hora atual.
        /// </summary>
        public TimeSpan Duration {
            get {
                var end = EndTime ?? DateTime.UtcNow;
                return end - StartTime;
            }
        }
    }
}
