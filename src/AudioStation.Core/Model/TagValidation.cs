using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    public class TagValidation : ITagValidation
    {
        public bool IsAlbumArtistValid { get; set; }
        public bool IsAlbumValid { get; set; }
        public bool IsTitleValid { get; set; }
        public bool IsGenreValid { get; set; }
        public bool IsTrackValid { get; set; }
        public bool IsTrackTotalValid { get; set; }
        public bool IsDiscNumberValid { get; set; }
        public bool IsDiscTotalValid { get; set; }

        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }

        public TagValidation()
        {
            this.ValidationMessage = string.Empty;
        }
    }
}
