using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Model
{
    public class LibraryDirectory : ILibraryDirectory
    {
        public string DirectoryLabel { get; set; }
        public string Directory { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsReadOnly { get; set; }
        public bool DeleteUnusedFolders { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
        public string CustomGroupingFormat { get; set; }
        public string CustomNamingFormat { get; set; }

        public LibraryDirectory()
        {
            this.DirectoryLabel = string.Empty;
            this.Directory = string.Empty;
            this.GroupingType = TrackGroupingType.None;
            this.NamingType = TrackNamingType.None;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.CustomNamingFormat = string.Empty;
            this.CustomGroupingFormat = string.Empty;
        }
    }
}
