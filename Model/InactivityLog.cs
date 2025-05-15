using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_WPF.Model
{
    class InactivityLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TotalInactivity { get; set; }
        public TimeSpan MaxInactivity { get; set; }
    }
}
