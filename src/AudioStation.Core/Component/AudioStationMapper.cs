using AudioStation.Core.Component.Interface;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IAudioStationMapper))]
    public class AudioStationMapper : IAudioStationMapper
    {
        private readonly ILoggerFactory _loggerFactory;

        Dictionary<AudioStationMapperConfiguration, AudioStationMapperConfiguration> _configurations;

        [IocImportingConstructor]
        public AudioStationMapper(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IAudioStationMapperConfiguration ConfigureMap<TSource, TDest>(TSource source, TDest destination, IAudioStationMapper.MapType type = IAudioStationMapper.MapType.Permissive)
        {
            var configuration = new AudioStationMapperConfiguration(typeof(TSource), typeof(TDest));

            // Configuration Cache
            _configurations.Add(configuration, configuration);

            // -> User will configure for specifics
            return configuration;
        }

        public TDest Map<TSource, TDest>(TSource source, IAudioStationMapper.MapType type = IAudioStationMapper.MapType.Permissive)
        {
            throw new NotImplementedException();
        }

        public void MapOnto<TSource, TDest>(TSource source, TDest destination, IAudioStationMapper.MapType type = IAudioStationMapper.MapType.Permissive)
        {
            throw new NotImplementedException();
        }
    }
}
