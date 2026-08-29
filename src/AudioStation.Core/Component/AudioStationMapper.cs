using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IAudioStationMapper))]
    public class AudioStationMapper : IAudioStationMapper
    {
        private readonly ILoggerFactory _loggerFactory;

        Dictionary<int, IMapper> _mappers;

        [IocImportingConstructor]
        public AudioStationMapper(ILoggerFactory loggerFactory)
        {
            _mappers = new Dictionary<int, IMapper>();
            _loggerFactory = loggerFactory;
        }

        public TDest Map<TSource, TDest>(TSource source)
        {
            try
            {
                var destination = Activator.CreateInstance(typeof(TDest));

                var mapper = GetMapper<TSource, TDest>();

                return (TDest)mapper.Map(source, destination, typeof(TSource), typeof(TDest));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping objects:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public void MapOnto<TSource, TDest>(TSource source, TDest dest)
        {
            try
            {
                var mapper = GetMapper<TSource, TDest>();

                mapper.Map(source, dest, typeof(TSource), typeof(TDest));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping objects:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private IMapper GetMapper<TSource, TDest>()
        {
            var hashCode = HashCode.Combine(typeof(TSource), typeof(TDest));

            if (_mappers.ContainsKey(hashCode))
                return _mappers[hashCode];

            try
            {
                //var sourceProperties = typeof(TSource).GetProperties().Where(x => x.PropertyType.Has<IgnoreAttribute>()).Select(x => x.Name).ToList();
                //var destProperties = typeof(TDest).GetProperties().Where(x => x.PropertyType.Has<IgnoreAttribute>()).Select(x => x.Name).ToList();

                var config = new MapperConfiguration(cfg =>
                {
                    var map = cfg.CreateMap<TSource, TDest>();

                }, _loggerFactory);

                var mapper = config.CreateMapper();

                _mappers.Add(hashCode, mapper);

                return mapper;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error creating type mapper: {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
