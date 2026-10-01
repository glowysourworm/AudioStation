namespace AudioStation.Core.Model.Interface
{
    public interface ILibraryDirectory
    {
        public string Directory { get; set; }
        public string DirectoryLabel { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsReadOnly { get; set; }
        public bool DeleteUnusedFolders { get; set; }
        public TrackGroupingType FolderFormatType { get; set; }
        public TrackNamingType FileFormatType { get; set; }
        public string CustomFolderFormat { get; set; }
        public string CustomFileFormat { get; set; }
    }
}
