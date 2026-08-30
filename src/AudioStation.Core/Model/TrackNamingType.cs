using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudioStation.Core.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrackNamingType : int
    {
        [Display(Name = "None", ShortName = "None", Description = "Keep file names as is")]
        None = 0,

        /// <summary>
        /// Naming follows the track title from the tag and includes the track number
        /// </summary>
        [Display(Name = "Standard", ShortName = "Standard", Description = "[Track Number] [Track Name].[File Extension]")]
        Standard = 1,

        /// <summary>
        /// Naming has artist, album, and track title, including track number, from the 
        /// tag data.
        /// </summary>
        [Display(Name = "Descriptive", ShortName = "Descriptive", Description = "[Artist]-[Album]-[Track Name]-[Track Number].[File Extension]")]
        Descriptive = 2
    }
}
