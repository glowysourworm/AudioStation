namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Converts the properties of one type to another during mapping (this is optional using the fluent method)
    /// </summary>
    public interface IAudioStationMapperConverter<TSource, TDest>
    {
        TDest ConvertTo(TSource source);

        TSource ConvertFrom(TDest dest);
    }
}
