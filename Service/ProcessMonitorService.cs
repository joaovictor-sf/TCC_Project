using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TCC_MVVM.Model;

namespace TCC_MVVM.Service {
    class ProcessMonitorService {
        public event Action<List<ProcessLog>>? MonitoringStopped;

        private readonly List<ProcessSession> _processSessions = new();
        private readonly TimeSpan _monitoringInterval;
        private System.Timers.Timer? _timer;
        private DateTime _monitoringStartTime;

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

            // Fecha sessões de processos que não estão mais ativos
            foreach (var session in _processSessions.Where(s => !s.EndTime.HasValue)) {
                if (!activeProcesses.ContainsKey(session.Key)) {
                    session.EndTime = now;
                }
            }

            // Inicia novas sessões para processos recém-detectados
            foreach (var kvp in activeProcesses) {
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

            // Finaliza todas as sessões ativas
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
                    StartTime = g.Min(s => s.StartTime.ToUniversalTime()),
                    Timestamp = DateTime.UtcNow
                })
                .OrderByDescending(p => p.UsageTime)
                .ToList();
        }
    }
}