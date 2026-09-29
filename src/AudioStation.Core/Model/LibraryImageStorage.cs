using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Specifies image storage options for the library
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LibraryImageStorage
    {
        [Display(Name = "Tag", Description = "This option will set AudioStation to store album art in the audio file's tag")]
        Tag = 0,

        [Display(Name = "Folder", Description = "This option will set AudioStation to store artwork in the audio file's folder (typically the album)")]
        Folder = 1,

        [Display(Name = "Application Folder", Description = "This option will set AudioStation to store artwork in the audio file's special folders")]
        ApplicationFolder = 2
    }
}
