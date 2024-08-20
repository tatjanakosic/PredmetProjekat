using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.MethodProfiler
{
    public class Profiler
    {
        private readonly ConcurrentDictionary<string, MethodProfile> _profiles;
        private readonly ThreadLocal<Stopwatch> _stopwatch;
        public Profiler()
        {
            _profiles = new ConcurrentDictionary<string, MethodProfile>();
            _stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());
        }

        public void StartProfiling(string methodName)
        {
            _stopwatch.Value.Restart();
        }

        public void StopProfiling(string methodName)
        {
            _stopwatch.Value.Stop();
            _profiles.AddOrUpdate(methodName,
                new MethodProfile
                {
                    MethodName = methodName,
                    CallCount = 1,
                    TotalExecutionTime = _stopwatch.Value.ElapsedMilliseconds,
                    MemoryUsage = GC.GetTotalMemory(false)
                },
                (key, existingProfile) =>
                {
                    existingProfile.CallCount++;
                    existingProfile.TotalExecutionTime += _stopwatch.Value.ElapsedMilliseconds;
                    existingProfile.MemoryUsage += GC.GetTotalMemory(false);
                    return existingProfile;
                });
        }

        public List<MethodProfile> GetProfiles()
        {
            return _profiles.Values.ToList();
        }

        public List<MethodProfile> GetProfilesData()
        {
            // Returns the profiles in a serializable format

            return _profiles.Values
                             .Select(p => new MethodProfile
                             {
                                 MethodName = p.MethodName,
                                 CallCount = p.CallCount,
                                 TotalExecutionTime = p.TotalExecutionTime,
                                 MemoryUsage = p.MemoryUsage
                             })
                             .ToList();
        }

        public void ClearProfiles()
        {
            _profiles.Clear();
        }
    }
}
