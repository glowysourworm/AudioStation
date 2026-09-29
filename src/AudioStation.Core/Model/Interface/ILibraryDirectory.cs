namespace AudioStation.Core.Model.Interface
{
    public interface ILibraryDirectory
    {
        public string Directory { get; set; }
        public string DirectoryLabel { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsReadOnly { get; set; }
        public bool DeleteUnusedFolders { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
        public string CustomGroupingFormat { get; set; }
        public string CustomNamingFormat { get; set; }
    }
}
