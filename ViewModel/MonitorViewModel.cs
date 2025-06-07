using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.Service;
using TCC_MVVM.Util;

namespace TCC_MVVM.ViewModel
{
    class MonitorViewModel : ViewModelBase {
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

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public MonitorViewModel(UserModel usuario) {
            _usuarioLogado = usuario;

            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        public MonitorViewModel() {
            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
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
            /*foreach (var log in summary) {
                log.UserId = _usuarioLogado.Id;
            }
            context.ProcessLogs.AddRange(summary);*/

            foreach (var log in summary) {
                var data = DateTime.UtcNow.Date;

                var existing = context.ProcessLogs.FirstOrDefault(pl => 
                    pl.UserId == _usuarioLogado.Id &&
                    pl.AppName == log.AppName &&
                    pl.WindowTitle == log.WindowTitle &&
                    pl.Date == data);

                if (existing != null) {
                    existing.UsageTime += log.UsageTime;
                    //existing.Timestamp = DateTime.UtcNow.TimeOfDay;
                } else {
                    log.UserId = _usuarioLogado.Id;
                    log.Date = data;
                    //log.Timestamp = DateTime.UtcNow.TimeOfDay;
                    context.ProcessLogs.Add(log);
                }
            }
            context.SaveChanges();
        }

        private void SaveInactivityLogToDatabase() {
            /*using var context = new AppDbContext();
            var inactivityLog = _idleMonitorService.GenerateLog();
            inactivityLog.UserId = _usuarioLogado.Id;
            context.InactivityLogs.Add(inactivityLog);
            context.SaveChanges();*/

            using var context = new AppDbContext();
            var newLog = _idleMonitorService.GenerateLog();
            newLog.UserId = _usuarioLogado.Id;

            var existing = context.InactivityLogs.FirstOrDefault(l =>
                l.UserId == _usuarioLogado.Id &&
                l.Date == newLog.Date);

            if (existing != null) {
                existing.TotalInactivity += newLog.TotalInactivity;
                if (newLog.MaxInactivity > existing.MaxInactivity)
                    existing.MaxInactivity = newLog.MaxInactivity;
            } else {
                context.InactivityLogs.Add(newLog);
            }

            context.SaveChanges();
        }
    }
}
