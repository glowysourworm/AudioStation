using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class UpdateEqualizerGainEventData
    {
        public float Frequency { get; set; }
        public float Gain { get; set; }
    }
    public class UpdateEqualizerGainEvent : IocEvent<UpdateEqualizerGainEventData>
    {
    }
}
