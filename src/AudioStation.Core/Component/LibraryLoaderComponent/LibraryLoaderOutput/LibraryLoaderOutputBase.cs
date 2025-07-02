using AudioStation.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderOutputBase
    {
        /// <summary>
        /// Work Item Log
        /// </summary>
        public List<LogMessage> Log { get; }

        public LibraryLoaderOutputBase()
        {
            this.Log = new List<LogMessage>();
        }
    }
}
