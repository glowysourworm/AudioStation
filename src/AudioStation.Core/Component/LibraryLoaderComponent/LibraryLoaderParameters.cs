namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// Generic set of parameters needed to initialize and run a new library loader task
    /// </summary>
    public class LibraryLoaderParameters
    {
        /// <summary>
        /// Type of loader task to run
        /// </summary>
        public LibraryLoadType LoadType { get; set; }

        /// <summary>
        /// Directory out of which to run the task
        /// </summary>
        public string Directory { get; set; }
    }
}
