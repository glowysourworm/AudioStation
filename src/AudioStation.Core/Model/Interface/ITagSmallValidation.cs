namespace AudioStation.Core.Model.Interface
{
    /// <summary>
    /// Interface to represent tag field validation.
    /// </summary>
    public interface ITagSmallValidation
    {
        bool IsAlbumArtistValid { get; set; }
        bool IsAlbumValid { get; set; }
        bool IsTitleValid { get; set; }
        bool IsGenreValid { get; set; }
        bool IsTrackValid { get; set; }
        bool IsTrackTotalValid { get; set; }
        bool IsMediaNumberValid { get; set; }
        bool IsMediaTotalValid { get; set; }
        bool IsMediaFormatValid { get; set; }
        bool IsDurationMillisecondsValid { get; set; }
        bool IsYearValid { get; set; }

        /// <summary>
        /// Represents overall validation in one property
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// Message to the user  about validation
        /// </summary>
        string ValidationMessage { get; set; }
    }
}
