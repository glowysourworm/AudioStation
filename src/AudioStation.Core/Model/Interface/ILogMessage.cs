using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Model.Interface
{
    public interface ILogMessage
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public LogMessageType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
