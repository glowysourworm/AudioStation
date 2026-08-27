using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Model
{
    public enum TrackNamingType
    {
        [Display(Name = "None", ShortName = "Name", Description = "No Preference")]
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
