namespace AudioStation.Core.Service
{
    public class RateLimitInfo
    {
        public string ServiceName { get; private set; }
        public int AllowedRequsts { get; set; }
        public int RemainingRequests { get; set; }
        public DateTimeOffset LastRequest { get; set; }
        public DateTimeOffset ResetAt { get; set; }
        public bool IsSet { get; set; }

        public RateLimitInfo(string serviceName)
        {
            this.ServiceName = serviceName;
        }
    }
}
