using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.Model.DTO;
using TCC_MVVM.Service;
using TCC_MVVM.MVVM.Commands;
using TCC_MVVM.View;
using TCC_MVVM.MVVM.Base;
using TCC_MVVM.Util;

namespace TCC_MVVM.MVVM.ViewModel
{
    class MonitorViewModel : ViewModelBase {
        private readonly ProcessMonitorService _processMonitorService;
        private readonly IdleMonitorService _idleMonitorService;
        private readonly UserModel _usuarioLogado;

        public ObservableCollection<ProcessDisplayItem> ProcessosMonitorados { get; } = new();

        private DispatcherTimer _timer;
        private DateTime _lastTick;
        private DailyWorkLog _todayLog;

        private TimeSpan _workedToday;
        public TimeSpan RemainingTime => _usuarioLogado.WorkHours.ToTimeSpan() - _workedToday;

        private string _tempoRestante = "00:00:00";
        public string TempoRestante {
            get => _tempoRestante;
            set {
                _tempoRestante = value;
                OnPropertyChanged();
            }
        }

        private bool _hasCompletedWorkToday;
        public bool HasCompletedWorkToday {
            get => _hasCompletedWorkToday;
            set {
                _hasCompletedWorkToday = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        private bool _isMonitoring;
        public bool IsMonitoring {
            get => _isMonitoring;
            private set {
                if (SetProperty(ref _isMonitoring, value)) {
                    CommandManager.InvalidateRequerySuggested();
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

        public ICommand LogoutCommand { get; }

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public MonitorViewModel(UserModel usuario) {
            _usuarioLogado = usuario;

            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring && !HasCompletedWorkToday);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            LogoutCommand = new RelayCommand(ExecuteLogout, _ => !IsMonitoring);

            if (!(DateTime.UtcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)) {
                using var db = new AppDbContext();
                var hoje = DateTime.UtcNow.ToLocalTime().Date;
                _todayLog = db.DailyWorkLogs
                    .FirstOrDefault(l => l.UserId == _usuarioLogado.Id && l.Date == hoje);

                _workedToday = _todayLog?.TimeWorked ?? TimeSpan.Zero;
                if (_workedToday >= _usuarioLogado.WorkHours.ToTimeSpan()) {
                    HasCompletedWorkToday = true;
                }

                UpdateRemainingTimeUI();
            }

            _processMonitorService.ProcessesUpdated += AtualizarListaMonitorada;
        }

        //Apagar metodo
        public MonitorViewModel() {
            _processMonitorService = new ProcessMonitorService(TimeSpan.FromSeconds(5));

            _idleMonitorService = new IdleMonitorService();

            _processMonitorService.MonitoringStopped += SaveLogsToDatabase;

            StartCommand = new RelayCommand(() => IsMonitoring = true, () => !IsMonitoring && !HasCompletedWorkToday);
            StopCommand = new RelayCommand(() => IsMonitoring = false, () => IsMonitoring);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            LogoutCommand = new RelayCommand(ExecuteLogout, _ => !IsMonitoring);

            _processMonitorService.ProcessesUpdated += AtualizarListaMonitorada;
        }

        private void ExecuteLogout(object? parameter) {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginView = new LoginView();
                if (loginView.DataContext is LoginViewModel loginVM) {
                    loginVM.Reset();
                }
                loginView.Show();

                foreach (Window window in Application.Current.Windows) {
                    if (window.DataContext == this) {
                        window.Close();
                        break;
                    }
                }
            });
        }

        private void AtualizarListaMonitorada(List<(string AppName, string WindowTitle)> processos) {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var novos = processos.Select(p => new ProcessDisplayItem(p.AppName, p.WindowTitle)).ToList();

                foreach (var existente in ProcessosMonitorados.ToList()) {
                    if (!novos.Contains(existente))
                        ProcessosMonitorados.Remove(existente);
                }

                foreach (var novo in novos) {
                    if (!ProcessosMonitorados.Contains(novo))
                        ProcessosMonitorados.Add(novo);
                }
            });
        }


        private void InicializarTimer() {
            using var db = new AppDbContext();

            var hoje = DateTime.UtcNow.Date;
            _todayLog = db.DailyWorkLogs
                .FirstOrDefault(l => l.UserId == _usuarioLogado.Id && l.Date.Date == hoje);

            if (_todayLog == null) {
                _todayLog = new DailyWorkLog
                {
                    UserId = _usuarioLogado.Id,
                    Date = hoje,
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
                IsMonitoring = false;
                HasCompletedWorkToday = true;

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
                var data = DateTime.UtcNow.ToLocalTime().Date;

                var existing = context.ProcessLogs.FirstOrDefault(pl => 
                    pl.UserId == _usuarioLogado.Id &&
                    pl.AppName == log.AppName &&
                    pl.WindowTitle == log.WindowTitle &&
                    pl.Date == data);

                if (existing != null) {
                    existing.UsageTime += log.UsageTime;
                } else {
                    log.UserId = _usuarioLogado.Id;
                    log.Date = data;
                    context.ProcessLogs.Add(log);
                }
            }
            context.SaveChanges();
        }

        private void SaveInactivityLogToDatabase() {
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
