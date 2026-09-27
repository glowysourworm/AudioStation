using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public enum ServiceComponentRequestType
    {
        Execute,
        Load,
        Reset
    }

    public class ServiceComponentRequestData
    {
        public Guid ComponentId { get; set; }
        public Guid? ComponentPartId { get; set; }
        public ServiceComponentRequestType Type { get; set; }
        public bool ShowProgress { get; set; }
    }

    public class ServiceComponentRequestEvent : IocEvent<ServiceComponentRequestData>
    { }
}
