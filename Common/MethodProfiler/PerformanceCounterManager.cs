using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MethodProfiler
{
    public class PerformanceCounterManager
    {
        private PerformanceCounter _cpuCounter;

        public PerformanceCounterManager()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        public float GetCurrentCpuUsage()
        {
            return _cpuCounter.NextValue();
        }
    }
}
