namespace AudioStation.Core.Component.Interface
{
    public interface IAudioStationMapperConfiguration
    {
        void IgnoreSourceProperty(string name);
        void IgnoreDestinationProperty(string name);
    }
}
