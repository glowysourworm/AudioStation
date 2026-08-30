using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudioStation.Core.Model
{
    public enum LibraryEntryType : int
    {
        Track = 0,
        Album,
        Artist,
        Genre
    }

    /// <summary>
    /// TODO: This has to be better integrated. What other types are there?
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrackType : int
    {
        Any = 0,
        Music = 1,
        AudioBook = 2
    }
}
