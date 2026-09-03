using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Model
{
    public enum LibraryImportType
    {
        /// <summary>
        /// Source folder is taken into the library as a new library folder; and the files are handled in place.
        /// </summary>
        [Display(Name = "In Place (no file migration)", ShortName = "InPlaceDirectory", Description = "This option will cause Audio Station to keep this folder as a library folder; and no file migration will be performed")]
        InPlaceDirectory = 0,

        /// <summary>
        /// Files will be migrated to a destination folder and managed - giving AudioStation the ability to create / delete files and folders.
        /// </summary>
        [Display(Name = "File Migration", ShortName = "Migration", Description = "This option will cause Audio Station to perform a file migration; and to manage the library folder more thoroughly, with options to delete files and folders.")]
        Migration = 1
    }
}
