namespace AudioStation.Core.Model.Interface
{
    public interface ILibraryDirectory
    {
        public string DirectoryLabel { get; set; }
        public string Directory { get; set; }
        public bool IsPrimary { get; set; }
        public TrackType TrackType { get; set; }
        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }
    }
}
