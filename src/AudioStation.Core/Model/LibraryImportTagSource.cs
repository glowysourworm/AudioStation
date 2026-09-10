using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudioStation.Core.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LibraryImportTagSource
    {
        /// <summary>
        /// Source folder is taken into the library as a new library folder; and the files are handled in place.
        /// </summary>
        [Display(Name = "File", ShortName = "File", Description = "The tag data from the source file will be preferred over other data sources")]
        File = 0,

        /// <summary>
        /// Files will be migrated to a destination folder and managed - giving AudioStation the ability to create / delete files and folders.
        /// </summary>
        [Display(Name = "Data Service", ShortName = "DataService", Description = "The tag data from data services will be preferred over other data sources (i.e. Music Brainz, etc..)")]
        DataService = 1
    }
}
