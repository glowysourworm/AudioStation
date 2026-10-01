using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Format attribute for using ITrackSmall formatting to add field data
    /// </summary>
    public class LibraryFormatAttribute : System.Attribute
    {
        /// <summary>
        /// Name of the format attribute:  (e.g. Genre is {genre}) The token characters are chosen by code, also.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Token Name of the format attribute:  (e.g. MediaTotal is "media-total" implies {media-total}) The token characters are chosen by code!
        /// </summary>
        public string TokenName { get; set; }

        /// <summary>
        /// Description of the format attribute:  (e.g. MediaTotal is "The total media count for this track's release")
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Format argument may be applied to files
        /// </summary>
        public LibraryFormatUse Use { get; set; }
    }
}
