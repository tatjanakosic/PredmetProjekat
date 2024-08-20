using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MethodProfiler
{
    [Serializable]
    public class MethodProfile
    {
        public string MethodName { get; set; }
        public int CallCount { get; set; }
        public long TotalExecutionTime { get; set; }
        public long MemoryUsage { get; set; }
        public double AverageExecutionTime => CallCount > 0 ? (double)TotalExecutionTime / CallCount : 0;
    }
}
