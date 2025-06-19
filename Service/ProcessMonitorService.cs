using System.Diagnostics;
using TCC_MVVM.Model;

namespace TCC_MVVM.Service {
    /// <summary>
    /// Serviço responsável por monitorar os processos ativos do sistema, registrando sessões de uso por janela.
    /// Gera logs de uso que podem ser persistidos ao final do monitoramento.
    /// </summary>
    class ProcessMonitorService {
        /// <summary>
        /// Evento disparado quando o monitoramento é interrompido. Fornece um resumo dos logs gerados.
        /// </summary>
        public event Action<List<ProcessLog>>? MonitoringStopped;
        /// <summary>
        /// Evento disparado sempre que a lista de processos ativos é atualizada.
        /// </summary>
        public event Action<List<(string AppName, string WindowTitle)>>? ProcessesUpdated;

        /// <summary>
        /// Lista de sessões ativas e encerradas de processos monitorados.
        /// Cada sessão representa o tempo de uso de uma janela de aplicativo.
        /// </summary>
        private readonly List<ProcessSession> _processSessions = new();
        /// <summary>
        /// Intervalo entre as verificações de processos em execução.
        /// </summary>
        private readonly TimeSpan _monitoringInterval;
        /// <summary>
        /// Timer responsável por acionar a verificação de processos no intervalo definido.
        /// </summary>
        private System.Timers.Timer? _timer;

        /// <summary>
        /// Indica se o serviço está monitorando no momento.
        /// </summary>
        public bool IsMonitoring { get; private set; }
        /// <summary>
        /// Data/hora em que o monitoramento foi iniciado.
        /// </summary>
        public DateTime MonitoringStartTime { get; private set; }

        /// <summary>
        /// Cria uma nova instância do serviço de monitoramento de processos.
        /// </summary>
        /// <param name="monitoringInterval">Intervalo entre cada verificação de processos.</param>
        public ProcessMonitorService(TimeSpan monitoringInterval) {
            _monitoringInterval = monitoringInterval;
        }

        /// <summary>
        /// Inicia o monitoramento de processos. Ignorado se já estiver ativo.
        /// </summary>
        public void StartMonitoring() {
            if (IsMonitoring) return;

            MonitoringStartTime = DateTime.UtcNow;
            _processSessions.Clear();

            _timer = new System.Timers.Timer(_monitoringInterval.TotalMilliseconds);
            _timer.Elapsed += (s, e) => CheckProcesses();
            _timer.Start();
            IsMonitoring = true;
        }

        /// <summary>
        /// Verifica os processos ativos e atualiza a lista de sessões em andamento.
        /// </summary>
        private void CheckProcesses() {
            var now = DateTime.UtcNow;
            var activeProcesses = GetActiveProcesses();

            EncerrarSessoesFinalizadasExternamente(activeProcesses, now);
            AdicionarNovasSessoes(activeProcesses, now);
            NotificarAtualizacaoProcessos();
        }

        /// <summary>
        /// Finaliza sessões cujo processo deixou de estar ativo.
        /// </summary>
        private void EncerrarSessoesFinalizadasExternamente(Dictionary<string, (string AppName, string WindowTitle)> ativos, DateTime now) {
            foreach (var session in _processSessions.Where(s => !s.EndTime.HasValue)) {
                if (!ativos.ContainsKey(session.Key)) {
                    session.EndTime = now;
                }
            }
        }

        /// <summary>
        /// Cria novas sessões para processos recém iniciados.
        /// </summary>
        private void AdicionarNovasSessoes(Dictionary<string, (string AppName, string WindowTitle)> ativos, DateTime now) {
            foreach (var kvp in ativos) {
                if (!_processSessions.Any(s => s.Key == kvp.Key && !s.EndTime.HasValue)) {
                    _processSessions.Add(new ProcessSession
                    {
                        Key = kvp.Key,
                        AppName = kvp.Value.AppName,
                        WindowTitle = kvp.Value.WindowTitle,
                        StartTime = now
                    });
                }
            }
        }

        /// <summary>
        /// Notifica a interface gráfica com a lista atual de processos ativos.
        /// </summary>
        private void NotificarAtualizacaoProcessos() {
            var ativos = _processSessions
                .Where(s => !s.EndTime.HasValue)
                .Select(s => (s.AppName, s.WindowTitle))
                .Distinct()
                .ToList();

            ProcessesUpdated?.Invoke(ativos);
        }

        /// <summary>
        /// Obtém todos os processos com janelas visíveis no momento.
        /// </summary>
        /// <returns>Dicionário com chave única para cada processo com nome e título da janela.</returns>
        private Dictionary<string, (string AppName, string WindowTitle)> GetActiveProcesses() {
            return Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .Select(p =>
                {
                    try {
                        return new
                        {
                            Key = $"{p.ProcessName}|{p.MainWindowTitle}",
                            AppName = p.ProcessName,
                            WindowTitle = p.MainWindowTitle
                        };
                    } catch {
                        return null;
                    }
                })
                .Where(p => p != null)
                .ToDictionary(p => p.Key, p => (p.AppName, p.WindowTitle));
        }

        /// <summary>
        /// Interrompe o monitoramento, finaliza sessões abertas e dispara o evento <see cref="MonitoringStopped"/>.
        /// </summary>
        public void StopMonitoring() {
            if (!IsMonitoring || _timer == null) return;

            _timer.Stop();
            _timer.Dispose();
            IsMonitoring = false;

            var now = DateTime.UtcNow;
            foreach (var session in _processSessions.Where(s => !s.EndTime.HasValue)) {
                session.EndTime = now;
            }

            var summary = GenerateSummary();
            MonitoringStopped?.Invoke(summary);
        }

        /// <summary>
        /// Gera o resumo final do tempo de uso agrupado por aplicação e título de janela.
        /// </summary>
        /// <returns>Lista de <see cref="ProcessLog"/> resumidos por aplicação.</returns>
        private List<ProcessLog> GenerateSummary() {
            return _processSessions
                .GroupBy(s => new { s.AppName, s.WindowTitle })
                .Select(g => new ProcessLog
                {
                    AppName = g.Key.AppName,
                    WindowTitle = g.Key.WindowTitle,
                    UsageTime = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks)),
                    Date = DateTime.UtcNow.Date
                })
                .OrderByDescending(p => p.UsageTime)
                .ToList();
        }
    }
}