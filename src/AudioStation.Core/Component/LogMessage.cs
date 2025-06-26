
using Microsoft.Extensions.Logging;

namespace AudioStation.Model
{
    public enum LogMessageType
    {
        General,
        Vendor,
        LibraryLoader,
        LibraryLoaderWorkItem,          // Has specific Id per log message collection
        AudioProcessing,
        BitmapConversion,
        Database
    }
    public struct LogMessage
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public LogMessageType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public LogMessage() : 
            this(string.Empty, LogMessageType.General, LogLevel.Information)
        {
        }
        public LogMessage(string message, LogMessageType type) : 
            this(message, type, LogLevel.Information)
        {
            
        }
        public LogMessage(string message, LogMessageType type, LogLevel level)
        {
            this.Message = message;
            this.Level = level;
            this.Type = type;
            this.Timestamp = DateTime.Now;
        }
    }
}
