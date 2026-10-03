namespace AudioStation.Core.Component.LibraryLoaderComponent.Interface
{
    public interface ILibraryLoaderLoad
    {
        /// <summary>
        /// Owner identifier for the load
        /// </summary>
        Guid OwnerId { get; }

        /// <summary>
        /// Display name for the load (problem was with casting of the payload)
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Type of load to be instantiated and processed by the ILibraryLoader
        /// </summary>
        LibraryLoadType LoadType { get; }

        /// <summary>
        /// Returns the payload for the ILibraryLoaderLoad (type information is removed for the design. But, individual class
        /// hierarchies are used to implement specifics without too much trouble)
        /// </summary>
        object Payload { get; }
    }
}
