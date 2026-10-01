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
        public TrackGroupingType FolderFormatType { get; set; }
        public TrackNamingType FileFormatType { get; set; }
        public string CustomFolderFormat { get; set; }
        public string CustomFileFormat { get; set; }

        public LibraryDirectory()
        {
            this.DirectoryLabel = string.Empty;
            this.Directory = string.Empty;
            this.FolderFormatType = TrackGroupingType.None;
            this.FileFormatType = TrackNamingType.None;
            this.IsReadOnly = true;
            this.DeleteUnusedFolders = false;
            this.CustomFileFormat = string.Empty;
            this.CustomFolderFormat = string.Empty;
        }
    }
}
