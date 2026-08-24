using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderEntityOutput<T> : LibraryLoaderOutputBase where T : AudioStationEntityBase
    {
        /// <summary>
        /// Result entity for the operation
        /// </summary>
        public T Result { get; set; }
    }
}
