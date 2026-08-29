namespace AudioStation.Core.Component.Interface
{
    public interface IAudioStationMapper
    {
        public enum MapType
        {
            /// <summary>
            /// Allows errors and missing properties by; and skips exceptions involving those properties. 
            /// Any application level exceptions (things we didn't handle as part of mapping) will be thrown.
            /// </summary>
            Permissive = 0,

            /// <summary>
            /// Throws all errors
            /// </summary>
            Strict = 1
        }

        /// <summary>
        /// Maps properties onto the destination object using recursion. Any unmatched properties, or unmapped
        /// properties will be handled depending on the type (see MapType)
        /// </summary>
        public IAudioStationMapperConfiguration ConfigureMap<TSource, TDest>(TSource source, TDest destination, MapType type = MapType.Permissive);

        /// <summary>
        /// Maps all properties using property names. Returns a new object with mapped properties.
        /// </summary>
        public TDest Map<TSource, TDest>(TSource source, MapType type = MapType.Permissive);

        /// <summary>
        /// Maps all properties using property names to existing object.
        /// </summary>
        public void MapOnto<TSource, TDest>(TSource source, TDest destination, MapType type = MapType.Permissive);
    }
}
