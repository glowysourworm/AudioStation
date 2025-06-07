namespace AudioStation.Model
{
    public enum LogMessageSeverity
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }
    public enum LogMessageType
    {
        General,
        Database
    }
    public struct LogMessage
    {
        public string Message { get; set; }
        public LogMessageSeverity Severity { get; set; }
        public LogMessageType Type { get; set; }

        public LogMessage()
        {
            this.Message = string.Empty;
            this.Severity = LogMessageSeverity.Info;
            this.Type = LogMessageType.General;
        }

        public LogMessage(string message)
        {
            this.Message = message;
            this.Severity = LogMessageSeverity.Info;
            this.Type = LogMessageType.General;
        }
        public LogMessage(string message, LogMessageSeverity severity)
        {
            this.Message = message;
            this.Severity = severity;
            this.Type = LogMessageType.General;
        }
        public LogMessage(string message, LogMessageType type, LogMessageSeverity severity)
        {
            this.Message = message;
            this.Severity = severity;
            this.Type = type;
        }
    }
}
