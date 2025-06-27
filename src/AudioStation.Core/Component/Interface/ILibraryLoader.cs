using AudioStation.Core.Model;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    public interface ILibraryLoader : IDisposable
    {
        /// <summary>
        /// Sends updates from any work item (idle / in process)
        /// </summary>
        public event SimpleEventHandler<LibraryWorkItem> WorkItemUpdate;

        /// <summary>
        /// Sends an update to remind the UI dispatcher to check the ILibraryLoader for
        /// any work item updates.
        /// </summary>
        public event SimpleEventHandler ProcessingUpdate;

        PlayStopPause GetState();

        /// <summary>
        /// Gets a thread-safe copy of work items for an update from the loader core
        /// </summary>
        public IEnumerable<LibraryWorkItem> GetIdleWorkItems();

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
        /// Loads music brainz data from any possible vendor source; searches to ensure that the
        /// tag files from taglib are filled in; and stores the music brainz id's in their proper
        /// database location(s). The result should be a complete music brainz reference set with
        /// updated ID's. AcoustID is used as a fallback for sound fingerprinting to locate the 
        /// music brainz id per track.
        /// </summary>
        void LoadMusicBrainzRecordsAsync();

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
    }
}
