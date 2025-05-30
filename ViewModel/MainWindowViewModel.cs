using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.Service;
using TCC_MVVM.Util;

namespace TCC_MVVM.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged {
        private readonly ProcessMonitorService _processMonitorService;
        private readonly IdleMonitorService _idleMonitorService;
        private bool _isMonitoring;
        private readonly UserModel _usuarioLogado;

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public bool IsMonitoring {
            get => _isMonitoring;
            private set {
                if (SetProperty(ref _isMonitoring, value)) {
                    if (value) {
                        _processMonitorService.StartMonitoring();
                        _idleMonitorService.Start();
                    } else {
                        _processMonitorService.StopMonitoring();
                        _idleMonitorService.Stop();
                        SaveInactivityLogToDatabase();
                    }

                    OnPropertyChanged(nameof(IsNotMonitoring));
                }
            }
        }

        public bool IsNotMonitoring => !IsMonitoring;

        public MainWindowViewModel(UserModel usuario) {
            _usuarioLogado = usuario;

            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;
            //_processMonitorService.MonitoringStopped += SaveLogsToFile;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);
        }

        public MainWindowViewModel() {
            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;
            //_processMonitorService.MonitoringStopped += SaveLogsToFile;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);
        }

        private void SaveLogsToDatabase(List<ProcessLog> summary) {
            using var context = new AppDbContext();
            foreach (var log in summary) {
                log.UserId = _usuarioLogado.Id;
            }
            context.ProcessLogs.AddRange(summary);
            context.SaveChanges();
        }

        private void SaveInactivityLogToDatabase() {
            using var context = new AppDbContext();
            var inactivityLog = _idleMonitorService.GenerateLog();
            inactivityLog.UserId = _usuarioLogado.Id;
            context.InactivityLogs.Add(inactivityLog);
            context.SaveChanges();
        }

        /**
         * USAR APENAS PARA TESTES
         * Método para salvar os logs em um arquivo de texto.
         * O arquivo será salvo na mesma pasta do executável.
         */
        private void SaveLogsToFile(List<ProcessLog> summary) {
            string filePath = "work_log.txt";

            var content = new System.Text.StringBuilder();
            content.AppendLine($"Relatório de Trabalho - {DateTime.Now}");
            //content.AppendLine($"Período monitorado: de {_processMonitorService.MonitoringStartTime} até {DateTime.Now}");
            content.AppendLine("==========================================");

            // Agrupa por aplicativo e soma os tempos
            var appUsage = summary
                .GroupBy(p => p.AppName)
                .Select(g => new {
                    App = g.Key,
                    TotalTime = TimeSpan.FromTicks(g.Sum(p => p.UsageTime.Ticks)),
                    Windows = g.Select(p => p.WindowTitle).Distinct()
                })
                .OrderByDescending(a => a.TotalTime);

            foreach (var app in appUsage) {
                content.AppendLine($"{app.App} - Tempo total: {app.TotalTime:hh\\:mm\\:ss}");
                foreach (var window in app.Windows) {
                    content.AppendLine($"  - {window}");
                }
            }

            File.WriteAllText(filePath, content.ToString());
            MessageBox.Show($"Relatório salvo em:\n{Path.GetFullPath(filePath)}");
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
