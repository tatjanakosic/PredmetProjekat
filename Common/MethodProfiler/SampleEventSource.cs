using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MethodProfiler
{
    [EventSource(Name = "ServiceController")]
    public class SampleEventSource : EventSource
    {
        public static SampleEventSource Log = new SampleEventSource();

        [Event(1, Level = EventLevel.Informational)]
        public void MethodEnter(string methodName)
        {
            WriteEvent(1, methodName);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void MethodExit(string methodName)
        {
            WriteEvent(2, methodName);
        }
    }
}
