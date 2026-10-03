using AudioStation.Core.Component.LibraryLoaderComponent.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// This loader object is meant to contain the load and provide type checks
    /// </summary>
    public class LibraryLoaderLoad<T> : ILibraryLoaderLoad
    {
        T _payload;

        public Guid OwnerId { get; private set; }
        public string DisplayName { get; private set; }
        public LibraryLoadType LoadType { get; private set; }
        public T Payload { get { return _payload; } }
        object ILibraryLoaderLoad.Payload { get { return _payload; } }

        /// <summary>
        /// Loads the loader with the load specification.
        /// </summary>
        public LibraryLoaderLoad(Guid ownerId, LibraryLoadType loadType, string displayName, T payload)
        {
            _payload = payload;

            this.DisplayName = displayName;
            this.OwnerId = ownerId;
            this.LoadType = loadType;
        }
    }
}
