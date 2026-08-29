using AudioStation.Core.Component.Interface;

namespace AudioStation.Core.Component
{
    public class AudioStationMapperConfiguration : IAudioStationMapperConfiguration
    {
        Type _sourceType;
        Type _destinationType;

        List<string> _ignoreSource;
        List<string> _ignoreDestination;

        public AudioStationMapperConfiguration(Type sourceType, Type destinationType)
        {
            _ignoreSource = new List<string>();
            _ignoreDestination = new List<string>();
            _sourceType = sourceType;
            _destinationType = destinationType;
        }

        public void IgnoreSourceProperty(string name)
        {
            _ignoreSource.Add(name);
        }

        public void IgnoreDestinationProperty(string name)
        {
            _ignoreDestination.Add(name);
        }

        public Type GetSourceType()
        {
            return _sourceType;
        }
        public Type GetDestinationType()
        {
            return _destinationType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_sourceType, _destinationType);
        }
        public override bool Equals(object? obj)
        {
            var other = obj as AudioStationMapperConfiguration;

            return _sourceType == other.GetSourceType() &&
                _destinationType == other.GetDestinationType();
        }
    }
}
