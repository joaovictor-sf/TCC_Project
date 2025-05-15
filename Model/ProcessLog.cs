using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_WPF.Model
{
    class ProcessLog
    {
        public int Id { get; set; }
        public string AppName { get; set; }
        public string WindowTitle { get; set; }
        public TimeSpan CpuTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
