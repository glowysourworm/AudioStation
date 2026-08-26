using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Output
{
    public class LibraryLoaderEntityOutput<T> where T : AudioStationEntityBase
    {
        /// <summary>
        /// Result entity for the operation
        /// </summary>
        public T Result { get; set; }
    }
}
