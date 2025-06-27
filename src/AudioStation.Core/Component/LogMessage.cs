
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
        public bool IsSeparatedLog {  get; set; }
        public object LogComponentId { get; set; }
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
        public LogMessage(string message, LogMessageType type, LogLevel level) :
            this(false, type, message, type, level)
        {
        }
        public LogMessage(bool isSeparated, object componentId, string message, LogMessageType type, LogLevel level)
        {
            this.IsSeparatedLog = isSeparated;
            this.LogComponentId = componentId;
            this.Message = message;
            this.Level = level;
            this.Type = type;
            this.Timestamp = DateTime.Now;
        }
    }
}
