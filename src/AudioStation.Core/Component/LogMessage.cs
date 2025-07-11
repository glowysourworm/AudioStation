
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;

namespace AudioStation.Model
{
    public enum LogMessageType
    {
        [Display(Name = "General", Description = "Application log messages about user interaction")]
        General,

        [Display(Name = "Vendor", Description = "Application log messages about 3rd party libraries")]
        Vendor,

        [Display(Name = "Library Loader", Description = "Application log messages about the operation of the library loader")]
        LibraryLoader,

        [Display(Name = "Library Loader Work Item", Description = "Application log messages about a particular library loader work item")]
        LibraryLoaderWorkItem,          // Has specific Id per log message collection

        [Display(Name = "Audio Processing", Description = "Application log messages about audio processing")]
        AudioProcessing,

        [Display(Name = "Bitmap Conversion", Description = "Application log messages about the conversion of bitmap types from external sources")]
        BitmapConversion,

        [Display(Name = "File Tag Update", Description = "Application log messages about any changes to file tags")]
        FileTagUpdate,

        [Display(Name = "Database", Description = "Application log messages about the database")]
        Database,

        [Display(Name = "Other (Component)", Description = "Application log messages about other 3rd party components")]
        OtherComponent
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
