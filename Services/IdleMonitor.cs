using System.Runtime.InteropServices;
using System.Timers;

namespace TCC_WPF.Services
{
    class IdleMonitor
    {
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        struct LASTINPUTINFO {
            public uint cbSize;
            public uint dwTime;
        }

        private readonly System.Timers.Timer _timer;
        private readonly TimeSpan _threshold = TimeSpan.FromSeconds(10);
        private DateTime? _inactivityStart = null;

        public TimeSpan TotalInactivity { get; private set; } = TimeSpan.Zero;
        public TimeSpan MaxInactivity { get; private set; } = TimeSpan.Zero;

        public IdleMonitor() {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += CheckInactivity;
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private void CheckInactivity(object sender, ElapsedEventArgs e) {
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
    }
}
