using System.Diagnostics;
using TCC_MVVM.Model;

namespace TCC_MVVM.Service {
    class ProcessMonitorService {
        public event Action<List<ProcessLog>>? MonitoringStopped;
        public event Action<List<(string AppName, string WindowTitle)>>? ProcessesUpdated;

        private readonly List<ProcessSession> _processSessions = new();
        private readonly TimeSpan _monitoringInterval;
        private System.Timers.Timer? _timer;

        public bool IsMonitoring { get; private set; }
        public DateTime MonitoringStartTime { get; private set; }

        public ProcessMonitorService(TimeSpan monitoringInterval) {
            _monitoringInterval = monitoringInterval;
        }

        public void StartMonitoring() {
            if (IsMonitoring) return;

            MonitoringStartTime = DateTime.UtcNow;
            _processSessions.Clear();

            _timer = new System.Timers.Timer(_monitoringInterval.TotalMilliseconds);
            _timer.Elapsed += (s, e) => CheckProcesses();
            _timer.Start();
            IsMonitoring = true;
        }

        private void CheckProcesses() {
            var now = DateTime.UtcNow;
            var activeProcesses = GetActiveProcesses();

            EncerrarSessoesFinalizadasExternamente(activeProcesses, now);
            AdicionarNovasSessoes(activeProcesses, now);
            NotificarAtualizacaoProcessos();
        }

        private void EncerrarSessoesFinalizadasExternamente(Dictionary<string, (string AppName, string WindowTitle)> ativos, DateTime now) {
            foreach (var session in _processSessions.Where(s => !s.EndTime.HasValue)) {
                if (!ativos.ContainsKey(session.Key)) {
                    session.EndTime = now;
                }
            }
        }

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

        private void NotificarAtualizacaoProcessos() {
            var ativos = _processSessions
                .Where(s => !s.EndTime.HasValue)
                .Select(s => (s.AppName, s.WindowTitle))
                .Distinct()
                .ToList();

            ProcessesUpdated?.Invoke(ativos);
        }

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