using AudioStation.Core.Component.Interface;

namespace AudioStation.Core.Component
{
    public class AudioStationMapperConfiguration : IAudioStationMapperConfiguration
    {
        List<string> _ignoreSource;

        public Type SourceType { get; private set; }
        public Type DestinationType { get; private set; }

        public IEnumerable<Type> SourceInterfaceTypes { get { return _sourceInterfaces; } }

        List<Type> _sourceInterfaces;
        Dictionary<int, Action<IAudioStationMapper, object, object>> _propertyConverters;

        public AudioStationMapperConfiguration(Type sourceType, Type destinationType)
        {
            this.SourceType = sourceType;
            this.DestinationType = destinationType;

            _ignoreSource = new List<string>();
            _sourceInterfaces = new List<Type>();
            _propertyConverters = new Dictionary<int, Action<IAudioStationMapper, object, object>>();
        }

        #region (public) Fluent Methods
        public IAudioStationMapperConfiguration IgnoreSourceProperty(string name)
        {
            _ignoreSource.Add(name);

            return this;
        }
        public IAudioStationMapperConfiguration DeclareSourceInterface<TInterface>()
        {
            _sourceInterfaces.Add(typeof(TInterface));

            return this;
        }
        public IAudioStationMapperConfiguration DeclarePropertyConverter<VSource, VDest>(string name, Action<IAudioStationMapper, VSource, VDest> action)
            where VSource : class
            where VDest : class
        {
            var hashCode = CreateConverterHashCode(name, typeof(VSource), typeof(VDest));

            if (_propertyConverters.ContainsKey(hashCode))
                throw new Exception(string.Format("Trying to add duplicate property converter:  {0} -> {1}", typeof(VSource).Name, typeof(VDest).Name));

            // Stage the callback for mapping
            _propertyConverters.Add(hashCode, (mapper, source, dest) =>
            {
                action(mapper, source as VSource, dest as VDest);
            });

            return this;
        }
        #endregion

        public bool IsIgnoredSourceProperty(string name)
        {
            return _ignoreSource.Contains(name);
        }
        public bool HasPropertyConverter(string name, Type sourceType, Type destType)
        {
            return _propertyConverters.ContainsKey(CreateConverterHashCode(name, sourceType, destType));
        }
        public void RunPropertyConverter(string name, Type sourceType, Type destType, object? source, object? dest, IAudioStationMapper mapper)
        {
            // IT MUST WORK! @_@
            _propertyConverters[CreateConverterHashCode(name, sourceType, destType)](mapper, source, dest);
        }

        public Type GetSourceType()
        {
            return this.SourceType;
        }
        public Type GetDestinationType()
        {
            return this.DestinationType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.SourceType, this.DestinationType);
        }
        public override bool Equals(object? obj)
        {
            var other = obj as AudioStationMapperConfiguration;

            if (other == null)
                return false;

            return this.SourceType == other.GetSourceType() &&
                   this.DestinationType == other.GetDestinationType();
        }

        public bool HasSourceInterface<TInterface>()
        {
            return _sourceInterfaces.Any(x => x == typeof(TInterface));
        }

        private int CreateConverterHashCode(string propertyName, Type sourceType, Type destType)
        {
            return HashCode.Combine(propertyName, sourceType, destType);
        }
    }
}
