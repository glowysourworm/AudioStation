using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.IdSharp;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Class that represents a "full" tag - which will be all the fields AudioStation supports. This
    /// will only be loaded into memory when the user interacts with the UI; and not during library
    /// loading unless it is only for a temporary workload process. The cache performance will be 
    /// badly degraded if you try loading it while iterating files.
    /// </summary>
    public class TagFull : ITagFull
    {
        public string? AlbumArtist { get; set; }
        public string? Album { get; set; }
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public string? Copyright { get; set; }
        public string? Genre { get; set; }
        public int? TrackNumber { get; set; }
        public int? TrackTotal { get; set; }
        public int? MediaNumber { get; set; }
        public int? MediaTotal { get; set; }
        public string? MediaFormat { get; set; }
        public string? Publisher { get; set; }
        public int? DurationMilliseconds { get; set; }
        public string? SortAlbumArtist { get; set; }
        public string? SortArtist { get; set; }
        public string? SortAlbum { get; set; }
        public string? SortTitle { get; set; }
        public int? Year { get; set; }
        public IList<UserDefinedField> UserDefinedFields { get; set; }
        public IList<InvolvedPerson> InvolvedPeople { get; set; }

        public TagFull()
        {
            this.UserDefinedFields = new List<UserDefinedField>();
            this.InvolvedPeople = new List<InvolvedPerson>();
        }
    }
}
