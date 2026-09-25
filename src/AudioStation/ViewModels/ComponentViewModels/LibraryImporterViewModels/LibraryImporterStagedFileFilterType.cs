using System.ComponentModel.DataAnnotations;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public enum LibraryImporterStagedFileFilterType
    {
        [Display(Name = "All Files", Description = "All staged files will be shown")]
        None = 0,

        [Display(Name = "Valid Files", Description = "Only valid (for import) staged files will be shown")]
        Valid = 1,

        [Display(Name = "Invalid Files", Description = "Only invalid (cannot yet import) staged files will be shown")]
        Invalid = 2,

        [Display(Name = "AcoustID", Description = "Only staged files with AcoustID result(s) will be shown")]
        AcoustID = 3,

        [Display(Name = "Music Brainz (basic)", Description = "Only staged files with Music Brainz (basic) result(s) will be shown")]
        MusicBrainzBasic = 4,

        [Display(Name = "Music Brainz (special tag)", Description = "Only staged files with Music Brainz (special tag) result(s) will be shown")]
        MusicBrainzSpecialTag = 5,

        [Display(Name = "Library Conflict", Description = "Only staged files with library conflicts will be shown")]
        LibraryConflict = 6
    }
}
