using AudioStation.Core.Model;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    public interface ILibraryLoader
    {
        /// <summary>
        /// Log event, with a bool to represent whether it was an error
        /// </summary>
        public event SimpleEventHandler<string, bool> LogEvent;

        /// <summary>
        /// Loads the library of mp3 files (from disk only!) by reading their TagLib.File entity. This is done
        /// using the taglib standard .net library.
        /// </summary>
        /// <param name="baseDirectory">Base directory of the library. *.mp3 / recursive lookup is assumed.</param>
        /// <param name="maxNumberOfThreads">Max number of threads to apply to TPL</param>
        /// <returns>Loaded set of library entries (awaitable task)</returns>
        Task<List<LibraryEntry>> LoadLibraryAsync(string baseDirectory, int maxNumberOfThreads = 4);

        /// <summary>
        /// Loads M3U file from specified directory (tree) using M3U C# libraries. This process may take some time.
        /// </summary>
        /// <param name="baseDirectory">Base directory. *.m3u / recursive lookup is assumed.</param>
        /// <param name="maxNumberOfThreads">Max number of threads for TPL</param>
        /// <returns>Loaded M3U file data</returns>
        Task<List<RadioEntry>> LoadRadioAsync(string baseDirectory, int maxNumberOfThreads = 4);

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
