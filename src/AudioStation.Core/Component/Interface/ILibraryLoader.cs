using AudioStation.Core.Model;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    public interface ILibraryLoader : IDisposable
    {
        /// <summary>
        /// Log event, with a bool to represent whether it was an error
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsRemoved;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsAdded;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemCompleted;
        public event SimpleEventHandler<LibraryEntry> LibraryEntryLoaded;
        public event SimpleEventHandler<RadioEntry> RadioEntryLoaded;
        public event SimpleEventHandler<PlayStopPause> ProcessingChanged;
        public event SimpleEventHandler ProcessingComplete;

        PlayStopPause GetState();

        /// <summary>
        /// Stops the work queue
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts (or restarts) the work queue
        /// </summary>
        void Start();

        /// <summary>
        /// Clears the work queue - which will stop automatically when it is done. The
        /// worker thread will continue to run in the background, waiting for another
        /// re-start.
        /// </summary>
        void Clear();

        /// <summary>
        /// Loads the library of mp3 files (from disk only!) by reading their TagLib.File entity. This is done
        /// using the taglib standard .net library. An event is fired when the work is completed.
        /// </summary>
        /// <param name="baseDirectory">Base directory of the library. *.mp3 / recursive lookup is assumed.</param>
        /// <returns>Loaded set of library entries (awaitable task)</returns>
        void LoadLibraryAsync(string baseDirectory);

        /// <summary>
        /// Loads M3U file from specified directory (tree) using M3U C# libraries. This process may take some time. 
        /// Events are fired when items are completed.
        /// </summary>
        /// <param name="baseDirectory">Base directory. *.m3u / recursive lookup is assumed.</param>
        /// <returns>Loaded M3U file data</returns>
        void LoadRadioAsync(string baseDirectory);

        /// <summary>
        /// Loads a LibraryEntry from the provided mp3 file.
        /// </summary>
        LibraryEntry LoadLibraryEntry(string fileName);

        /// <summary>
        /// Loads a RadioEntry from an m3u file.
        /// </summary>
        RadioEntry LoadRadioEntry(string fileName);
    }
}
