using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderEntitySetOutput<T> : LibraryLoaderOutputBase where T : AudioStationEntityBase
    {
        public IEnumerable<T> ResultSet { get; set; }
    }
}
