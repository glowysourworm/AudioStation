using AudioStation.Core.Component.LibraryLoaderComponent.Output.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Output
{
    public class LibraryLoaderEntityOutput<T> : ILibraryLoaderOutput where T : AudioStationEntityBase
    {
        /// <summary>
        /// Result entity for the operation
        /// </summary>
        public T Result { get; set; }
    }
}
