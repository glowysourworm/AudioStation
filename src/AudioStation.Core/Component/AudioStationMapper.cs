using System.Collections;
using System.Reflection;

using AudioStation.Core.Component.Interface;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IAudioStationMapper))]
    public class AudioStationMapper : IAudioStationMapper
    {
        private readonly ILoggerFactory _loggerFactory;

        SimpleDictionary<int, AudioStationMapperConfiguration> _configurations;
        SimpleDictionary<Type, SimpleDictionary<string, PropertyInfo>> _propertyStore;

        [IocImportingConstructor]
        public AudioStationMapper(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _propertyStore = new SimpleDictionary<Type, SimpleDictionary<string, PropertyInfo>>();
            _configurations = new SimpleDictionary<int, AudioStationMapperConfiguration>();
        }

        public IAudioStationMapperConfiguration ConfigureMap<TSource, TDest>()
        {
            // Constraints:
            //
            // 1) TDest (cannot be) interface type
            //

            if (typeof(TDest).IsInterface)
                throw new Exception("Cannot map to an interface destination - must be a class or struct");

            var configuration = new AudioStationMapperConfiguration(typeof(TSource), typeof(TDest));

            // Configuration Cache
            _configurations.Add(configuration.GetHashCode(), configuration);

            // -> User will configure for specifics
            return configuration;
        }

        public TDest Map<TSource, TDest>(TSource source, IAudioStationMapper.MapType type = IAudioStationMapper.MapType.Permissive)
        {
            // Check for mapper configuration
            var configuration = GetMapperConfiguration<TSource, TDest>();

            try
            {
                var destination = Activator.CreateInstance<TDest>();

                MapImpl(source, destination, typeof(TSource), typeof(TDest), type);

                return destination;
            }
            catch (Exception ex)
            {
                throw new Exception("Error mapping types:  " + ex.Message);
            }
        }

        public void MapOnto<TSource, TDest>(TSource source, TDest destination, IAudioStationMapper.MapType type = IAudioStationMapper.MapType.Permissive)
        {
            // Check for mapper configuration
            var configuration = GetMapperConfiguration<TSource, TDest>();

            try
            {
                MapImpl(source, destination, typeof(TSource), typeof(TDest), type);
            }
            catch (Exception ex)
            {
                throw new Exception("Error mapping types:  " + ex.Message);
            }
        }

        private void MapImpl(object source, object destination, Type sourceType, Type destType, IAudioStationMapper.MapType mapType = IAudioStationMapper.MapType.Permissive)
        {
            // Check for mapper configuration
            var configuration = GetMapperConfiguration(sourceType, destType, true);

            var destProperties = GetProperties(destType);
            var sourceProperties = GetProperties(sourceType);

            foreach (var propertyName in sourceProperties.Keys)
            {
                // Source Property (ignored)
                if (configuration.IsIgnoredSourceProperty(propertyName))
                    continue;

                // Destination Property (not found)
                if (!HandlePermissivity(propertyName, !destProperties.ContainsKey(propertyName), false, mapType))
                    continue;

                var destinationProperty = destProperties[propertyName];
                var sourceProperty = sourceProperties[propertyName];

                // Destination Property (read only)
                if (!HandlePermissivity(propertyName, !destinationProperty.CanWrite, false, mapType))
                    continue;

                // Source Property (read not permitted)
                if (!HandlePermissivity(propertyName, !sourceProperty.CanRead, false, mapType))
                    continue;

                // Procedure: Check these in order
                //
                // 0) Custom Mapper:   Check configuration converters
                // 1) Reference Types: Mismatch (requires mapper + recursion)
                // 2) Collections:     Iterate -> Recurse
                // 3) Complex Types:   Null References | Constructors
                // 4) Primitives:      Direct setter
                //

                var sourcePropertyType = sourceProperty.PropertyType;
                var destPropertyType = destinationProperty.PropertyType;

                bool misMatch = sourcePropertyType != destPropertyType;

                bool isReferenceType = !IsPrimitive(sourcePropertyType);
                bool isSourceCollection = IsCollection(sourcePropertyType);
                bool isDestinationList = !IsPrimitive(destPropertyType) && destPropertyType.HasInterface<IList>();

                var sourcePropertyValue = sourceProperty.GetValue(source);
                var destPropertyValue = destinationProperty.GetValue(destination);

                // Custom Property Mapping
                if (configuration.HasPropertyConverter(propertyName, sourcePropertyType, destPropertyType))
                {
                    // -> (Recurse)
                    configuration.RunPropertyConverter(propertyName, sourcePropertyType, destPropertyType, sourcePropertyValue, destPropertyValue, this);
                    continue;
                }

                // Mismatch (any reference type)
                if (misMatch && isReferenceType)
                {
                    var propertyMapper = GetMapperConfiguration(sourcePropertyType, destPropertyType, false);

                    // Missing Mapper
                    if (!HandlePermissivity(propertyName, propertyMapper == null, false, mapType))
                        continue;
                }

                // Collection
                if (isSourceCollection)
                {
                    // Source Null
                    if (ReferenceEquals(sourcePropertyValue, null))
                        destinationProperty.SetValue(destination, null);

                    // Destination Null (construct)
                    else if (ReferenceEquals(destPropertyValue, null))
                    {
                        // Destination Constructor (Null)
                        if (!HandleReferenceConstruction(propertyName, destinationProperty.PropertyType, ref destPropertyValue))
                            continue;
                    }

                    // -> Recurse
                    MapCollection(sourceProperty, destinationProperty, sourcePropertyValue, destPropertyValue, mapType);
                }

                // Complex Type
                //
                else if (isReferenceType)
                {
                    // Source Null
                    if (ReferenceEquals(sourcePropertyValue, null))
                        destinationProperty.SetValue(destination, null);

                    // Destination Null
                    else if (ReferenceEquals(destPropertyValue, null))
                    {
                        // Destination Constructor (Null)
                        if (!HandleReferenceConstruction(propertyName, destinationProperty.PropertyType, ref destPropertyValue))
                            continue;
                    }

                    // -> Recurse
                    MapImpl(sourcePropertyValue, destPropertyValue, sourcePropertyType, destPropertyType, mapType);
                }

                // Primitive
                else
                    destinationProperty.SetValue(destination, sourcePropertyValue);
            }
        }

        private void MapCollection(PropertyInfo sourceProperty, PropertyInfo destProperty, object sourceValue, object destValue, IAudioStationMapper.MapType mapType)
        {
            var destinationList = destValue as IList;
            var destinationItemType = destProperty.PropertyType.GetGenericArguments().FirstOrDefault() ?? typeof(object);

            // Destination (clear)
            destinationList.Clear();

            // Source (loop | recurse)
            foreach (var item in sourceValue as IEnumerable)
            {
                // Procedure: Check these in order
                //
                // 0) Nulls:          Direct setter
                // 1) Primitives:     Direct setter
                // 2) Collections:    Recurse
                // 3) Complex Types:  Recurse
                //

                // Null (go ahead and mirror null to the destination)
                if (ReferenceEquals(item, null))
                {
                    destinationList.Add(null);
                    continue;
                }

                var itemType = item.GetType();
                bool isReferenceType = !itemType.IsPrimitive;

                // Recurse -> (construct destination item)
                if (isReferenceType)
                {
                    object? destItem = null;

                    // Construct
                    if (!HandleReferenceConstruction("Collection Item [" + destinationItemType.Name + "]", destinationItemType, ref destItem))
                        continue;

                    MapImpl(item, destItem, itemType, destinationItemType, mapType);
                }

                // Primitive
                else
                    destinationList.Add(item);
            }
        }

        private bool HandlePermissivity(string propertyName, bool errorCondition, bool mandatory, IAudioStationMapper.MapType mapType)
        {
            if (mandatory && errorCondition)
                throw new Exception("Error mapping property:  " + propertyName);

            else if (errorCondition)
            {
                if (mapType == IAudioStationMapper.MapType.Strict)
                    throw new Exception("Error mapping property:  " + propertyName);

                return false;
            }

            return true;
        }

        private bool HandleReferenceConstruction(string propertyName, Type propertyType, ref object? value)
        {
            try
            {
                if (value == null)
                    value = propertyType.GetConstructor(new Type[] { }).Invoke(new object[] { });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error constructing property:  " + propertyName);
            }
        }

        private IDictionary<string, PropertyInfo> GetProperties(Type type)
        {
            if (_propertyStore.ContainsKey(type))
                return _propertyStore[type];

            else
            {
                var properties = type.GetProperties();

                _propertyStore.Add(type, new SimpleDictionary<string, PropertyInfo>());

                foreach (var property in properties)
                {
                    _propertyStore[type].Add(property.Name, property);
                }

                return _propertyStore[type];
            }
        }

        private IAudioStationMapperConfiguration? GetMapperConfiguration(Type sourceType, Type destType, bool force = true)
        {
            foreach (var configuration in _configurations.Values)
            {
                if (sourceType == configuration.SourceType &&
                    destType == configuration.DestinationType)
                    return configuration;

                foreach (var sourceInterface in configuration.SourceInterfaceTypes)
                {
                    if (sourceInterface == sourceType)
                        return configuration;
                }
            }

            if (force)
                throw new Exception(string.Format("Missing mapper for (make sure to declare source interface(s)):  {0} -> {1}", sourceType.Name, destType.Name));

            return null;
        }

        private IAudioStationMapperConfiguration? GetMapperConfiguration<TSource, TDest>(bool force = true)
        {
            return GetMapperConfiguration(typeof(TSource), typeof(TDest), force);
        }

        private int CalculateHashCode(Type sourceType, Type destType)
        {
            return HashCode.Combine(sourceType, destType);
        }

        /// <summary>
        /// Code to return any "primitive" types. These will be treated as value types. The MSFT Type 
        /// definition is all over the place trying to describe CLR types; but it has nothing to do with
        /// the user end.
        /// </summary>
        private bool IsPrimitive(Type type)
        {
            return (type == typeof(bool)) ||
                   (type == typeof(byte)) ||
                   (type == typeof(long)) ||
                   (type == typeof(DateTime)) ||
                   (type == typeof(DateTimeOffset)) ||
                   (type == typeof(double)) ||
                   (type == typeof(ushort)) ||
                   (type == typeof(short)) ||
                   (type == typeof(uint)) ||
                   (type == typeof(int)) ||
                   (type == typeof(char)) ||
                   (type == typeof(string)) ||
                   (type.IsEnum);
        }

        private bool IsCollection(Type type)
        {
            return !IsPrimitive(type) && type.HasInterface<IEnumerable>();
        }
    }
}
