using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// Generic set of parameters needed to initialize and run a new library loader task
    /// </summary>
    public class LibraryLoaderParameters<TIn> where TIn : LibraryLoaderLoadBase
    {
        /// <summary>
        /// Type of loader task to run
        /// </summary>
        public LibraryLoadType LoadType { get; }

        /// <summary>
        /// The load presented to the library loader. This is setup by user selection for specific task files.
        /// </summary>
        public TIn Load { get; }

        public LibraryLoaderParameters(LibraryLoadType loadType, TIn load)
        {
            this.LoadType = loadType;
            this.Load = load;
        }
    }
}
