using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    public class TagValidation : ITagSmallValidation
    {
        public bool IsAlbumArtistValid { get; set; }
        public bool IsAlbumValid { get; set; }
        public bool IsTitleValid { get; set; }
        public bool IsGenreValid { get; set; }
        public bool IsTrackValid { get; set; }
        public bool IsTrackTotalValid { get; set; }
        public bool IsMediaNumberValid { get; set; }
        public bool IsMediaTotalValid { get; set; }
        public bool IsMediaFormatValid { get; set; }
        public bool IsDurationMillisecondsValid { get; set; }
        public bool IsYearValid { get; set; }

        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }


        public TagValidation()
        {
            this.ValidationMessage = string.Empty;
        }
    }
}
