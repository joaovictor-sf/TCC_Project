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
    /// <summary>
    /// ViewModel responsável por monitorar processos e inatividade do usuário,
    /// controlar o tempo de trabalho diário e atualizar a interface.
    /// </summary>
    class MonitorViewModel : ViewModelBase {
        /// <summary>
        /// Serviço responsável por monitorar os processos em execução.
        /// </summary>
        private readonly ProcessMonitorService _processMonitorService;
        /// <summary>
        /// Serviço responsável por monitorar a inatividade do usuário.
        /// </summary>
        private readonly IdleMonitorService _idleMonitorService;
        /// <summary>
        /// Dados do usuário atualmente logado.
        /// </summary>
        private readonly UserModel _usuarioLogado;

        /// <summary>
        /// Lista de processos monitorados exibida na interface.
        /// </summary>
        public ObservableCollection<ProcessDisplayItem> ProcessosMonitorados { get; } = new();

        /// <summary>
        /// Temporizador usado para contar o tempo de trabalho em intervalos regulares.
        /// </summary>
        private DispatcherTimer _timer;
        /// <summary>
        /// Armazena o horário da última atualização do temporizador.
        /// Usado para calcular o tempo decorrido.
        /// </summary>
        private DateTime _lastTick;
        /// <summary>
        /// Registro do log de trabalho do dia atual no banco de dados.
        /// </summary>
        private DailyWorkLog _todayLog;

        /// <summary>
        /// Tempo total trabalhado no dia corrente.
        /// </summary>
        private TimeSpan _workedToday;

        /// <summary>
        /// Tempo restante de trabalho formatado para exibição na interface.
        /// </summary>
        private string _tempoRestante = "00:00:00";
        public string TempoRestante {
            get => _tempoRestante;
            set {
                _tempoRestante = value;
                OnPropertyChanged();
            }
        }

        private bool _hasCompletedWorkToday;
        /// <summary>
        /// Indica se o usuário já completou seu horário de trabalho no dia.
        /// </summary>
        public bool HasCompletedWorkToday {
            get => _hasCompletedWorkToday;
            set {
                _hasCompletedWorkToday = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Comando para iniciar o monitoramento.
        /// </summary>
        public ICommand StartCommand { get; }
        /// <summary>
        /// Comando para parar o monitoramento.
        /// </summary>
        public ICommand StopCommand { get; }

        private bool _isMonitoring;
        /// <summary>
        /// Indica se o monitoramento está ativo.
        /// </summary>
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

        /// <summary>
        /// Indica se o monitoramento está inativo.
        /// </summary>
        public bool IsNotMonitoring => !IsMonitoring;

        /// <summary>
        /// Comando para realizar logout e retornar à tela de login.
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Comando para minimizar a janela.
        /// </summary>
        public ICommand MinimizeCommand { get; }
        /// <summary>
        /// Comando para fechar a janela.
        /// </summary>
        public ICommand CloseCommand { get; }
        /// <summary>
        /// Delegate chamado para minimizar a janela.
        /// </summary>
        public Action? MinimizeWindow { get; set; }
        /// <summary>
        /// Delegate chamado para fechar a janela.
        /// </summary>
        public Action? CloseWindow { get; set; }

        /// <summary>
        /// Construtor principal do ViewModel. Inicializa comandos, serviços e configura o estado da interface.
        /// </summary>
        /// <param name="usuario">Usuário atualmente logado.</param>
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

        /// <summary>
        /// Construtor padrão necessário para evitar exceções em tempo de execução.
        /// </summary>
        [Obsolete("Use o construtor com parâmetro UserModel.")]
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

        /// <summary>
        /// Realiza logout e retorna para a tela de login.
        /// </summary>
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

        /// <summary>
        /// Atualiza a lista de processos monitorados exibida na interface.
        /// </summary>
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

        /// <summary>
        /// Inicializa o temporizador e prepara o log diário.
        /// </summary>
        private void InicializarTimer() {
            PrepararLogDiario();
            ConfigurarTimer();
        }
        /// <summary>
        /// Verifica ou cria o log de trabalho do dia atual.
        /// </summary>
        private void PrepararLogDiario() {
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
        }
        /// <summary>
        /// Configura e inicia o temporizador de contagem do tempo trabalhado.
        /// </summary>
        private void ConfigurarTimer() {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => AtualizarTempoTrabalhado();
            _lastTick = DateTime.Now;
            _timer.Start();
        }

        /// <summary>
        /// Interrompe e reseta o temporizador.
        /// </summary>
        private void PausarTimer() {
            _timer?.Stop();
            _timer = null;
        }

        /// <summary>
        /// Atualiza o tempo trabalhado com base no intervalo entre ticks.
        /// </summary>
        private void AtualizarTempoTrabalhado() {
            AtualizarTempoDecorrido();
            AtualizarLogDiarioNoBanco();
            UpdateRemainingTimeUI();
            VerificarEncerramentoAutomatico();
        }

        /// <summary>
        /// Atualiza a variável de tempo trabalhado.
        /// </summary>
        private void AtualizarTempoDecorrido() {
            var now = DateTime.Now;
            var delta = now - _lastTick;
            _lastTick = now;

            _workedToday += delta;
        }

        /// <summary>
        /// Atualiza o registro no banco de dados com o tempo de trabalho mais recente.
        /// </summary>
        private void AtualizarLogDiarioNoBanco() {
            using var db = new AppDbContext();
            var log = db.DailyWorkLogs.FirstOrDefault(l => l.Id == _todayLog.Id);
            if (log != null) {
                log.TimeWorked = _workedToday;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Encerra automaticamente o monitoramento se o tempo de trabalho for atingido.
        /// </summary>
        private void VerificarEncerramentoAutomatico() {
            if (_workedToday >= _usuarioLogado.WorkHours.ToTimeSpan()) {
                _timer.Stop();
                IsMonitoring = false;
                HasCompletedWorkToday = true;

                MessageBox.Show("Parabéns! Você completou seu horário de trabalho diário!", "Completo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Atualiza o tempo restante exibido na UI.
        /// </summary>
        private void UpdateRemainingTimeUI() {
            var remaining = _usuarioLogado.WorkHours.ToTimeSpan() - _workedToday;
            TempoRestante = remaining > TimeSpan.Zero ? remaining.ToString(@"hh\:mm\:ss") : "00:00:00";
        }

        /// <summary>
        /// Salva ou atualiza os logs de processos no banco de dados.
        /// </summary>
        private void SaveLogsToDatabase(List<ProcessLog> summary) {
            using var context = new AppDbContext();

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

        /// <summary>
        /// Salva ou atualiza os logs de inatividade no banco de dados.
        /// </summary>
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
