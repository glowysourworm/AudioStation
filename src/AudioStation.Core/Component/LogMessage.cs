
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;

namespace AudioStation.Model
{
    public enum LogMessageType
    {
        [Display(Name = "General", Description = "Application log messages about user interaction")]
        General,

        [Display(Name = "Component", Description = "Application log messages about internal components of Audio Station")]
        Component,

        [Display(Name = "Service", Description = "Application log messages about service connections and configuration issues")]
        Service,

        [Display(Name = "Database", Description = "Application log messages from the databases")]
        Database,

        [Display(Name = "Other (Component)", Description = "Application log messages about other 3rd party components")]
        OtherComponent
    }
    public enum LogMessageComponentType
    {
        [Display(Name = "None", Description = "N/A")]
        None,

        [Display(Name = "Configuration", Description = "Configuration for the application")]
        ConfigurationManager,

        [Display(Name = "Library Loader", Description = "Application log messages about the operation of the library loader")]
        LibraryLoader,

        [Display(Name = "Library Loader Work Item", Description = "Application log messages about a particular library loader work item")]
        LibraryLoaderWorkItem,          // Has specific Id per log message collection

        [Display(Name = "Audio Processing", Description = "Application log messages about audio processing")]
        AudioProcessing,

        [Display(Name = "Bitmap Conversion", Description = "Application log messages about the conversion of bitmap types from external sources")]
        BitmapConversion,

        [Display(Name = "File Tag Update", Description = "Application log messages about any changes to file tags")]
        FileTagUpdate
    }
    public enum LogMessageServiceType
    {
        [Display(Name = "None", Description = "N/A")]
        None,

        [Display(Name = "AcoustID", Description = "AcoustID audio fingerprint service")]
        AcoustID,

        [Display(Name = "Bandcamp", Description = "Bandcamp audio streaming and information service")]
        Bandcamp,

        [Display(Name = "Discogs", Description = "Discogs music information database")]
        Discogs,

        [Display(Name = "Fanart", Description = "Fanart musician art and biography data service")]
        Fanart,

        [Display(Name = "iTunes", Description = "iTunes music metadata and streaming service")]
        iTunes,

        [Display(Name = "LastFm", Description = "LastFm music metadata and streaming service")]
        LastFm,

        [Display(Name = "Music Brainz", Description = "Music Brainz metadata, and tag information database")]
        MusicBrainz,

        [Display(Name = "Spotify", Description = "Spotify musician art and biography data service")]
        Spotify
    }

    public enum LogMessageDbType
    {
        [Display(Name = "None", Description = "N/A")]
        None,

        [Display(Name = "Audio STation Db", Description = "Audio Station's primary database")]
        AudioStation,

        [Display(Name = "Music Brainz Db", Description = "Database of Music Brainz data cache (Audio Station's copy)")]
        MusicBrainz
    }
    public struct LogMessage
    {
        public int LogId { get; }
        public string Message { get; set; }
        public Exception? Exception { get; set; }
        public LogLevel Level { get; set; }
        public LogMessageType Type { get; set; }
        public LogMessageComponentType ComponentType { get; set; }
        public LogMessageServiceType ServiceType { get; set; }
        public LogMessageDbType DatabaseType { get; set; }
        public DateTime Timestamp { get; set; }

        // Message Only
        public LogMessage() :
            this(string.Empty, LogMessageType.General, LogLevel.Information, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None)
        {
        }
        public LogMessage(string message) :
            this(message, LogMessageType.General, LogLevel.Information, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogMessageComponentType componentType) :
            this(message, LogMessageType.Component, LogLevel.Information, componentType, LogMessageServiceType.None, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogMessageServiceType serviceType) :
            this(message, LogMessageType.Service, LogLevel.Information, LogMessageComponentType.None, serviceType, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogMessageDbType dbType) :
            this(message, LogMessageType.Database, LogLevel.Information, LogMessageComponentType.None, LogMessageServiceType.None, dbType)
        {

        }

        // Message + Log Level
        public LogMessage(string message, LogLevel level) :
            this(message, LogMessageType.General, level, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogLevel level, LogMessageComponentType componentType) :
            this(message, LogMessageType.Component, level, componentType, LogMessageServiceType.None, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogLevel level, LogMessageServiceType serviceType) :
            this(message, LogMessageType.Service, level, LogMessageComponentType.None, serviceType, LogMessageDbType.None)
        {

        }
        public LogMessage(string message, LogLevel level, LogMessageDbType dbType) :
            this(message, LogMessageType.Database, level, LogMessageComponentType.None, LogMessageServiceType.None, dbType)
        {

        }



        // Message + Log Level + Exception
        public LogMessage(string message, LogLevel level, Exception? exception) :
            this(message, LogMessageType.General, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None, level, exception)
        {
        }
        public LogMessage(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception) :
            this(message, LogMessageType.Component, componentType, LogMessageServiceType.None, LogMessageDbType.None, level, exception)
        {

        }
        public LogMessage(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception) :
            this(message, LogMessageType.Service, LogMessageComponentType.None, serviceType, LogMessageDbType.None, level, exception)
        {

        }
        public LogMessage(string message, LogMessageDbType dbType, LogLevel level, Exception? exception) :
            this(message, LogMessageType.Database, LogMessageComponentType.None, LogMessageServiceType.None, dbType, level, exception)
        {

        }

        // Full Constructor Without Exception
        private LogMessage(string message,
                             LogMessageType type,
                             LogLevel level,
                             LogMessageComponentType componentType,
                             LogMessageServiceType serviceType,
                             LogMessageDbType dbType) :
            this(message, type, componentType, serviceType, dbType, level, null)
        {
        }

        // Full Constructor
        public LogMessage(string message,
                             LogMessageType type,
                             LogMessageComponentType componentType,
                             LogMessageServiceType serviceType,
                             LogMessageDbType dbType,
                             LogLevel level,
                             Exception? exception)
        {
            if (type == LogMessageType.Service &&
                serviceType == LogMessageServiceType.None)
            {
                throw new ArgumentException("Must specify service type for service log");
            }

            else if (type == LogMessageType.Component &&
                     componentType == LogMessageComponentType.None)
            {
                throw new ArgumentException("Must specify component type for component log");
            }
            else if (type == LogMessageType.Database &&
                     dbType == LogMessageDbType.None)
            {
                throw new ArgumentException("Must specify database type for database log");
            }

            this.LogId = HashCode.Combine(type, componentType, serviceType, dbType);
            this.Message = message;
            this.Type = type;
            this.ComponentType = componentType;
            this.ServiceType = serviceType;
            this.DatabaseType = dbType;
            this.Level = level;
            this.Exception = exception;
            this.Timestamp = DateTime.Now;
        }
    }
}
