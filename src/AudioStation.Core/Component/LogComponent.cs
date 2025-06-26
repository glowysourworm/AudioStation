using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component
{
    public class LogComponent
    {
        readonly List<LogMessage> _log;
        readonly int _logMax;

        public LogComponent(int logMax)
        {
            _log = new List<LogMessage>();
            _logMax = logMax;
        }

        public void Add(LogMessage logMessage)
        {
            _log.Insert(0, logMessage);

            if (_log.Count > _logMax)
                _log.RemoveAt(_log.Count - 1);
        }

        public IEnumerable<LogMessage> GetLatestLogs(LogMessageType type, LogLevel level, int count)
        {
            return _log.Where(log => log.Level >= level).Take(count);
        }

        public void Clear()
        {
            _log.Clear();
        }
    }
}
