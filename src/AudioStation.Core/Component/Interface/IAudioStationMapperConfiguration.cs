namespace AudioStation.Core.Component.Interface
{
    public interface IAudioStationMapperConfiguration
    {
        Type SourceType { get; }
        Type DestinationType { get; }

        IEnumerable<Type> SourceInterfaceTypes { get; }

        /// <summary>
        /// Declares a specific property converter
        /// </summary>
        /// <typeparam name="VSource">Property source value</typeparam>
        /// <typeparam name="VDest">Property destination value</typeparam>
        /// <param name="name">Property name</param>
        /// <param name="mapper">Converter method</param>
        IAudioStationMapperConfiguration DeclarePropertyConverter<VSource, VDest>(string name, Action<IAudioStationMapper, VSource, VDest> mapper)
            where VSource : class
            where VDest : class;

        /// <summary>
        /// Skips source property during mapping
        /// </summary>
        IAudioStationMapperConfiguration IgnoreSourceProperty(string name);

        /// <summary>
        /// Declares that this mapper configuration - identified by its source / destination types - will
        /// be also used for the source interface of type TInterface. The actual property definitions will
        /// be drawn from the source type used when the mapper is called.
        /// </summary>
        /// <typeparam name="TInterface">Source interface type</typeparam>
        IAudioStationMapperConfiguration DeclareSourceInterface<TInterface>();

        bool IsIgnoredSourceProperty(string name);
        bool HasSourceInterface<TInterface>();
        bool HasPropertyConverter(string name, Type sourceType, Type destType);
        void RunPropertyConverter(string name, Type sourceType, Type destType, object? source, object? dest, IAudioStationMapper mapper);
    }
}
