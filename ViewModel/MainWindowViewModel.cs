using System.Diagnostics;
using System.IO;
using System.Windows;
using TCC_WPF.Infra;
using TCC_WPF.Model;
using TCC_WPF.Services;

namespace TCC_WPF.ViewModel
{
    class MainWindowViewModel
    {
        // Lista para armazenar os dados coletados
        private List<ProcessLog> _logs = new List<ProcessLog>();
        private IdleMonitor _idleMonitor = new IdleMonitor();

        public void StartMonitoring() {
            _idleMonitor.Start();
        }

        public void StopMonitoring() {
            _idleMonitor.Stop();
        }

        public void SaveInactivityLogToDatabase() {
            using var context = new AppDbContext();

            var inactivityLog = new InactivityLog
            {
                Timestamp = DateTime.UtcNow,
                TotalInactivity = _idleMonitor.TotalInactivity,
                MaxInactivity = _idleMonitor.MaxInactivity
            };

            context.InactivityLogs.Add(inactivityLog);
            context.SaveChanges();
        }

        public void SaveInactivityReportToFile() {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "inatividade_relatorio.txt");

            var lines = new List<string>
            {
                $"Relatório de Inatividade - {DateTime.Now}",
                $"Tempo total de inatividade: {_idleMonitor.TotalInactivity}",
                $"Maior tempo de inatividade detectado: {_idleMonitor.MaxInactivity}"
            };

            File.WriteAllLines(filePath, lines);
        }

        public void SaveLogsToDatabase() {
            var summary = _logs
                .GroupBy(log => new { log.AppName, log.WindowTitle })
                .Select(group => new ProcessLog
                {
                    AppName = group.Key.AppName,
                    WindowTitle = group.Key.WindowTitle,
                    CpuTime = TimeSpan.FromTicks(group.Sum(x => x.CpuTime.Ticks)),
                    StartTime = group.Min(x => x.StartTime.ToUniversalTime()),
                    Timestamp = DateTime.UtcNow
                })
                .ToList();

            using var context = new AppDbContext();
            context.ProcessLogs.AddRange(summary);
            context.SaveChanges();
        }

        public List<ProcessLog> CollectActiveProcesses() {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .Select(p => new ProcessLog
                {
                    AppName = p.ProcessName,
                    WindowTitle = p.MainWindowTitle,
                    CpuTime = p.TotalProcessorTime,
                    StartTime = p.StartTime,
                    Timestamp = DateTime.Now
                })
                .ToList();

            _logs.AddRange(processes);

            return _logs;
        }
    }
}
