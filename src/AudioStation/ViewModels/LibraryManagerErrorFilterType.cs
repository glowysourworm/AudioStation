using System.ComponentModel.DataAnnotations;

namespace AudioStation.ViewModels
{
    public enum LibraryManagerErrorFilterType
    {
        [Display(Name = "None", Description = "Do not apply any extra filtering to library results")]
        None,

        [Display(Name = "File Load Error", Description = "Search only for entries that had errors loading their files")]
        FileLoadError,

        [Display(Name = "File Un-Available", Description = "Search only for entries that have a missing file")]
        FileUnavailable
    }
}
