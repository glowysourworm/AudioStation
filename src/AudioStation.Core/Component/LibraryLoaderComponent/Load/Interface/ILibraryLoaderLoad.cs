namespace AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface
{
    public interface ILibraryLoaderLoad
    {
        /// <summary>
        /// Owner identifier for the load
        /// </summary>
        int OwnerId { get; }

        /// <summary>
        /// Type of load to be instantiated and processed by the ILibraryLoader
        /// </summary>
        LibraryLoadType Type { get; }
    }
}
