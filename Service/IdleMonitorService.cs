using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Threading;
using TCC_MVVM.Model;

namespace TCC_MVVM.Service {
    class IdleMonitorService {
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        struct LASTINPUTINFO {
            public uint cbSize;
            public uint dwTime;
        }

        private readonly DispatcherTimer _timer;
        private readonly TimeSpan _threshold = TimeSpan.FromSeconds(5); //Muda o tempo de inatividade aqui
        private DateTime? _inactivityStart = null;

        public TimeSpan TotalInactivity { get; private set; } = TimeSpan.Zero;
        public TimeSpan MaxInactivity { get; private set; } = TimeSpan.Zero;

        public IdleMonitorService() {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += CheckInactivity;
        }
            
        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

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

        private uint GetIdleTime() {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);

            return (uint)Environment.TickCount - lastInputInfo.dwTime;
        }

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
