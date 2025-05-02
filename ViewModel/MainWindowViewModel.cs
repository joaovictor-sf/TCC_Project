using System.Diagnostics;
using System.IO;
using System.Windows;
using TCC_WPF.Infra;
using TCC_WPF.Model;

namespace TCC_WPF.ViewModel
{
    class MainWindowViewModel
    {
        // Lista para armazenar os dados coletados
        private List<UserData> _logs = new List<UserData>();

        public void SaveLogsToDatabase() {
            var summary = _logs
                .GroupBy(log => new { log.AppName, log.WindowTitle })
                .Select(group => new UserData
                {
                    AppName = group.Key.AppName,
                    WindowTitle = group.Key.WindowTitle,
                    CpuTime = TimeSpan.FromTicks(group.Sum(x => x.CpuTime.Ticks)),
                    StartTime = group.Min(x => x.StartTime.ToUniversalTime()),
                    Timestamp = DateTime.UtcNow
                })
                .ToList();

            using var context = new AppDbContext();
            context.UserDatas.AddRange(summary);
            context.SaveChanges();
        }

        public List<UserData> CollectActiveProcesses() {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .Select(p => new UserData
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
