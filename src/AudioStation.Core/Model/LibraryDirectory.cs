using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    public class LibraryDirectory : ILibraryDirectory
    {
        public string DirectoryLabel { get; set; }
        public string Directory { get; set; }
        public bool IsPrimary { get; set; }
        public TrackType TrackType { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }

        public LibraryDirectory()
        {
            this.DirectoryLabel = string.Empty;
            this.Directory = string.Empty;
            this.TrackType = TrackType.Any;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;
        }
    }
}
