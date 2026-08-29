namespace AudioStation.Core.Component.Interface
{
    public interface IAudioStationMapper
    {
        /// <summary>
        /// Maps the source to a new destination object using the AutoMapper (see Ignore attribute for ignored properties)
        /// </summary>
        TDest Map<TSource, TDest>(TSource source);

        /// <summary>
        /// Maps the source to a onto a destination object using the AutoMapper (see Ignore attribute for ignored properties)
        /// </summary>
        void MapOnto<TSource, TDest>(TSource source, TDest dest);
    }
}
