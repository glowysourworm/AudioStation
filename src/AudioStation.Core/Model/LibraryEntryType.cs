using System.ComponentModel.DataAnnotations;

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
    public enum TrackCategory : int
    {
        [Display(Name = "Any (category)", Description = "Track may be of any category (e.g. Music, Audio Books, ...")]
        Any = 0,

        [Display(Name = "Music (category)", Description = "Track should be a music track")]
        Music = 1,

        [Display(Name = "Audio Book (category)", Description = "Track should be an audio book track")]
        AudioBook = 2
    }
}
