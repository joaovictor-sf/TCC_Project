using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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

        private DispatcherTimer _timer;
        private DateTime _lastTick;
        private DailyWorkLog _todayLog;

        private TimeSpan _workedToday;
        public TimeSpan RemainingTime => _usuarioLogado.WorkHours.ToTimeSpan() - _workedToday;

        private string _tempoRestante;
        public string TempoRestante {
            get => _tempoRestante;
            set {
                _tempoRestante = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public bool IsMonitoring {
            get => _isMonitoring;
            private set {
                if (SetProperty(ref _isMonitoring, value)) {
                    if (value) {
                        _processMonitorService.StartMonitoring();
                        _idleMonitorService.Start();
                        InicializarTimer();
                    } else {
                        _processMonitorService.StopMonitoring();
                        _idleMonitorService.Stop();
                        PausarTimer();
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

            InicializarTimer();
        }

        private void InicializarTimer() {
            if (_timer != null || DateTime.Today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return;

            using var db = new AppDbContext();

            var hoje = DateTime.Now.Date;
            _todayLog = db.DailyWorkLogs
                .FirstOrDefault(l => l.UserId == _usuarioLogado.Id && l.Date.Date == hoje);

            if (_todayLog == null) {
                _todayLog = new DailyWorkLog
                {
                    UserId = _usuarioLogado.Id,
                    Date = DateTime.Today,
                    TimeWorked = TimeSpan.Zero
                };
                db.DailyWorkLogs.Add(_todayLog);
                db.SaveChanges();
            }

            _workedToday = _todayLog.TimeWorked;
            UpdateRemainingTimeUI();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => AtualizarTempoTrabalhado();
            _lastTick = DateTime.Now;
            _timer.Start();
        }

        private void PausarTimer() {
            _timer?.Stop();
            _timer = null;
        }

        private void AtualizarTempoTrabalhado() {
            var now = DateTime.Now;
            var delta = now - _lastTick;
            _lastTick = now;

            _workedToday += delta;

            using var db = new AppDbContext();
            var log = db.DailyWorkLogs.FirstOrDefault(l => l.Id == _todayLog.Id);
            if (log != null) {
                log.TimeWorked = _workedToday;
                db.SaveChanges();
            }

            UpdateRemainingTimeUI();

            if (_workedToday >= _usuarioLogado.WorkHours.ToTimeSpan()) {
                _timer.Stop();
                MessageBox.Show("Parabéns! Você completou seu horário de trabalho diário!", "Completo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateRemainingTimeUI() {
            var remaining = _usuarioLogado.WorkHours.ToTimeSpan() - _workedToday;
            TempoRestante = remaining > TimeSpan.Zero ? remaining.ToString(@"hh\:mm\:ss") : "00:00:00";
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
