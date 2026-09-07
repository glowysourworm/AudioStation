namespace AudioStation.Core.Model.Interface
{
    public interface ILibraryDirectory
    {
        public string Directory { get; set; }
        public string DirectoryLabel { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsReadOnly { get; set; }
        public bool DeleteUnusedFolders { get; set; }
        public TrackCategory TrackCategory { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
        public LibraryImportType ImportType { get; set; }
        public IAudioEncoderInfo FormatPreference { get; set; }
    }
}
