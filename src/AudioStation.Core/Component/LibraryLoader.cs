using System.Collections.Concurrent;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;

using m3uParser;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.NativeIO.FastDirectory;

namespace AudioStation.Core.Component
{
    public class LibraryLoader : ILibraryLoader
    {
        const string UNKNOWN = "Unknown";

        public event SimpleEventHandler<string, bool> LogEvent;

        public Task<List<LibraryEntry>> LoadLibraryAsync(string baseDirectory, int maxNumberOfThreads = 4)
        {
            return Task<List<LibraryEntry>>.Run(() =>
            {
                // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
                var files = FastDirectoryEnumerator.GetFiles(baseDirectory, "*.mp3", SearchOption.AllDirectories);

                var entries = new ConcurrentBag<LibraryEntry>();

                // Use TPL to create library entries
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (file) =>
                {
                    LibraryEntry entry = null;

                    // Generate entry
                    try
                    {
                        // SHOULD BE THREAD SAFE
                        entry = this.LoadLibraryEntry(file.Path);

                        OnLog(string.Format("Music file loaded:  {0}", file.Path), false);
                    }
                    catch (Exception ex)
                    {
                        entry = new LibraryEntry()
                        {
                            FileName = file.Path,
                            FileError = true,
                            FileErrorMessage = ex.Message,        // Should be handled based on exception (user friendly message)
                        };

                        OnLog(string.Format("Music file load error:  {0}", file.Path), true);
                    }

                    // Keep track of completed entries 
                    if (entry != null)
                        entries.Add(entry);

                    else
                        throw new Exception("Unhandled library loader exception:  LibraryLoader.cs");
                });

                // Report
                OnLog(string.Format("{0} music files read successfully! {1} of {2} loaded. {3} had loading issues.",
                        entries.Count,
                        entries.Where(x => !x.FileError).Count(),
                        entries.Count,
                        entries.Where(x => x.FileError).Count()),
                        entries.Where(x => x.FileError).Count() != 0);

                OnLog("See Library Manager to resolve loading issues", false);

                return entries.ToList();
            });
        }

        public Task<List<RadioEntry>> LoadRadioAsync(string baseDirectory, int maxNumberOfThreads = 4)
        {
            return Task<List<RadioEntry>>.Run(() =>
            {
                // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
                var files = FastDirectoryEnumerator.GetFiles(baseDirectory, "*.m3u", SearchOption.AllDirectories);

                var entries = new ConcurrentBag<RadioEntry>();
                var errorCount = 0;
                var totalStreamCount = 0;

                // Use TPL to create library entries
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (file) =>
                {
                    RadioEntry entry = null;

                    // Generate entry
                    try
                    {
                        // SHOULD BE THREAD SAFE
                        entry = this.LoadRadioEntry(file.Path);

                        totalStreamCount += entry.Streams.Count;

                        OnLog(string.Format("Radio M3U file loaded:  {0}", file.Path), false);
                    }
                    catch (Exception ex)
                    {
                        OnLog(string.Format("Radio M3U file load error:  {0}", file.Path), true);
                    }

                    // Keep track of completed entries 
                    if (entry != null)
                        entries.Add(entry);

                    else
                        errorCount++;
                });

                // Report
                OnLog(string.Format("{0} M3U files read successfully! {1} of {2} loaded. {3} total streams loaded.",
                        files.Count(),
                        entries.Count(),
                        files.Count(),
                        totalStreamCount),
                        errorCount > 0);

                OnLog("See Library Manager to resolve loading issues", false);

                return entries.ToList();
            });
        }

        public LibraryEntry LoadLibraryEntry(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            try
            {

                var fileRef = TagLib.File.Create(file);

                return new LibraryEntry()
                {
                    FileName = file,
                    PrimaryArtist = fileRef.Tag.FirstAlbumArtist,
                    PrimaryGenre = fileRef.Tag.FirstGenre,

                    //AlbumArt = new SortedObservableCollection<SerializableBitmap>(fileRef.Tag.Pictures.Select(x => SerializableBitmap.ReadIPicture(x))),
                    Album = Format(fileRef.Tag.Album),
                    Disc = fileRef.Tag.Disc,
                    FileError = fileRef.PossiblyCorrupt,
                    FileErrorMessage = fileRef.CorruptionReasons.Join(",", x => x),
                    Title = Format(fileRef.Tag.Title),
                    Track = fileRef.Tag.Track
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public RadioEntry LoadRadioEntry(string fileName)
        {
            // Adding a nested try / catch for these files
            var fileContents = File.ReadAllText(fileName);
            var m3uFile = M3U.Parse(fileContents);

            if (m3uFile != null)
            {
                return new RadioEntry()
                {
                    Name = m3uFile.Attributes.TvgName,
                    Streams = m3uFile.Medias.Select(media => new RadioEntryStreamInfo()
                    {
                        Name = media.Title.RawTitle,
                        Homepage = media.Attributes.UrlTvg,
                        Endpoint = media.MediaFile,
                        LogoEndpoint = media.Attributes.TvgLogo
                    }).ToList()
                };
            }

            return null;
        }

        private string Format(string tagField)
        {
            return string.IsNullOrWhiteSpace(tagField) ? UNKNOWN : tagField;
        }

        private void OnLog(string message, bool error)
        {
            if (this.LogEvent != null)
                this.LogEvent(message, error);
        }
    }
}
