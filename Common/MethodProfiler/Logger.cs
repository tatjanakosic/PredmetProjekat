using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MethodProfiler
{
    public class Logger
    {
        private readonly string _logFilePath;
        private bool _disposed = false;

        public Logger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void LogProfiles(List<MethodProfile> profiles)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Logger");
            }

            using (var writer = new StreamWriter(_logFilePath, true))
            {
                foreach (var profile in profiles)
                {
                    writer.WriteLine($"Method: {profile.MethodName}, Call Count: {profile.CallCount}, Total Execution Time: {profile.TotalExecutionTime}, Average Execution Time: {profile.AverageExecutionTime}, Memory Usage: {profile.MemoryUsage}");
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here if needed
                }

                // Free any unmanaged resources here if needed
                _disposed = true;
            }
        }

        ~Logger()
        {
            Dispose(false);
        }
    }
}
