using System.Runtime.InteropServices;
using System.Windows.Threading;
using TCC_MVVM.Model;

namespace TCC_MVVM.Service {
    /// <summary>
    /// Serviço responsável por monitorar a inatividade do usuário no sistema operacional.
    /// Utiliza a API do Windows para detectar o tempo desde a última interação do usuário.
    /// </summary>
    class IdleMonitorService {
        /// <summary>
        /// Importa a função nativa do Windows que retorna o tempo (em ticks) desde a última entrada do usuário (teclado ou mouse).
        /// </summary>
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Estrutura usada pela função nativa GetLastInputInfo para armazenar informações sobre a última entrada do usuário.
        /// </summary>
        struct LASTINPUTINFO {
            public uint cbSize;
            public uint dwTime;
        }

        /// <summary>
        /// Intervalo mínimo para considerar o usuário como inativo.
        /// </summary>
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Tempo mínimo de inatividade necessário para considerar que o usuário está inativo.
        /// </summary>
        private static readonly TimeSpan _threshold = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Marca o início do período de inatividade atual, caso esteja em andamento.
        /// </summary>
        private DateTime? _inactivityStart = null;

        /// <summary>
        /// Soma total de todos os períodos de inatividade detectados durante a sessão.
        /// </summary>
        public TimeSpan TotalInactivity { get; private set; } = TimeSpan.Zero;
        /// <summary>
        /// Maior período contínuo de inatividade detectado.
        /// </summary>
        public TimeSpan MaxInactivity { get; private set; } = TimeSpan.Zero;

        /// <summary>
        /// Inicializa o serviço e configura o timer de verificação de inatividade.
        /// </summary>
        public IdleMonitorService() {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += CheckInactivity;
        }

        /// <summary>
        /// Inicia a verificação periódica de inatividade do usuário.
        /// </summary>
        public void Start() => _timer.Start();

        /// <summary>
        /// Interrompe a verificação periódica de inatividade do usuário.
        /// </summary>
        public void Stop() => _timer.Stop();

        /// <summary>
        /// Verifica o tempo atual de inatividade do usuário e atualiza os registros internos.
        /// </summary>
        private void CheckInactivity(object sender, EventArgs e) {
            uint idleMs = GetIdleTime();
            TimeSpan currentIdle = TimeSpan.FromMilliseconds(idleMs);

            if (currentIdle >= _threshold) {
                if (_inactivityStart == null)
                    _inactivityStart = DateTime.Now - currentIdle;

                if (currentIdle > MaxInactivity)
                    MaxInactivity = currentIdle;
            } else {
                if (_inactivityStart != null) {
                    TimeSpan period = DateTime.Now - _inactivityStart.Value;
                    TotalInactivity += period;

                    if (period > MaxInactivity)
                        MaxInactivity = period;

                    _inactivityStart = null;
                }
            }
        }

        /// <summary>
        /// Obtém o tempo (em milissegundos) desde a última interação do usuário com o sistema.
        /// </summary>
        private uint GetIdleTime() {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);

            return (uint)Environment.TickCount - lastInputInfo.dwTime;
        }

        /// <summary>
        /// Gera um registro de inatividade baseado nos dados coletados desde o início da sessão.
        /// </summary>
        /// <returns>Objeto <see cref="InactivityLog"/> contendo os dados agregados.</returns>
        public InactivityLog GenerateLog() {
            return new InactivityLog
            {
                Date = DateTime.UtcNow.Date,
                TotalInactivity = TotalInactivity,
                MaxInactivity = MaxInactivity
            };
        }
    }
}
