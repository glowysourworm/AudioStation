using System.IO;

using AudioStation.Model.Comparer;
using AudioStation.ViewModel;
using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Event;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Model
{
    [Serializable]
    public class Library : ViewModelBase
    {
        public event SimpleEventHandler<string, LogMessageType, LogMessageSeverity> LogEvent;

        SortedObservableCollection<LibraryEntry> _allTitles;
        SortedObservableCollection<LibraryEntry> _validTitles;
        SortedObservableCollection<ArtistViewModel> _validArtists;

        /// <summary>
        /// List of all library entries (non-grouped / sorted)
        /// </summary>
        public SortedObservableCollection<LibraryEntry> AllTitles
        {
            get { return _allTitles; }
            set { this.RaiseAndSetIfChanged(ref _allTitles, value); }
        }

        /// <summary>
        /// Titles that have adequate information to be used by AudioStation, to play and organize,
        /// library entries.
        /// </summary>
        public SortedObservableCollection<LibraryEntry> ValidTitles
        {
            get { return _validTitles; }
            set { this.RaiseAndSetIfChanged(ref _validTitles, value); }
        }

        /// <summary>
        /// Valid titles will add entries to this list - one per artist
        /// </summary>
        public SortedObservableCollection<ArtistViewModel> ValidArtists
        {
            get { return _validArtists; }
            set { this.RaiseAndSetIfChanged(ref _validArtists, value); }
        }

        public Library()
        {
            this.AllTitles = new SortedObservableCollection<LibraryEntry>(new LibraryEntryDefaultComparer());
            this.ValidTitles = new SortedObservableCollection<LibraryEntry>(new LibraryEntryDefaultComparer());
            this.ValidArtists = new SortedObservableCollection<ArtistViewModel>(new ArtistViewModelDefaultComparer());
        }

        /// <summary>
        /// Combined add method to update all library collections
        /// </summary>
        public void Add(LibraryEntry entry)
        {
            // All Titles
            this.AllTitles.Add(entry);

            var valid = ValidateEntry(entry);

            // Valid Titles (At least one artist, valid track data, etc..)
            if (valid)
                this.ValidTitles.Add(entry);

            // Valid Artists
            if (valid)
            {
                var artistEntry = this.ValidArtists.FirstOrDefault(x => x.Artist == entry.AlbumArtists.First().Name);

                // Existing
                if (artistEntry != null)
                {
                    // Artist -> Album(s)
                    if (!artistEntry.Albums.Any(x => x.Album == entry.Album))
                    {
                        var albumEntry = new AlbumViewModel()
                        {
                            Album = entry.Album,
                            Artist = entry.PrimaryArtist,
                            FileNameRef = entry.FileName,
                            Year = entry.Year,
                            Duration = entry.Duration
                        };

                        // Album -> Track(s)
                        albumEntry.Tracks.Add(new TitleViewModel()
                        {
                            FileName = entry.FileName,
                            Entry = entry,
                            Name = entry.Title,
                            Track = entry.Track,
                            Duration = entry.Duration
                        });

                        artistEntry.Albums.Add(albumEntry);
                    }
                    else
                    {
                        // Album -> Track(s)
                        var albumEntry = artistEntry.Albums.First(x => x.Album == entry.Album);
                        albumEntry.Duration.Add(entry.Duration);
                        albumEntry.Tracks.Add(new TitleViewModel()
                        {
                            FileName = entry.FileName,
                            Entry = entry,
                            Name = entry.Title,
                            Track = entry.Track,
                            Duration = entry.Duration
                        });
                    }
                }

                // New
                else
                {
                    var albumEntry = new AlbumViewModel()
                    {
                        Album = entry.Album,
                        Artist = entry.PrimaryArtist,
                        FileNameRef = entry.FileName,
                        Year = entry.Year
                    };
                    artistEntry = new ArtistViewModel()
                    {
                        Artist = entry.AlbumArtists.First().Name,
                        FileNameRef = entry.FileName
                    };

                    albumEntry.Tracks.Add(new TitleViewModel()
                    {
                        FileName = entry.FileName,
                        Entry = entry,
                        Name = entry.Title,
                        Track = entry.Track,
                        Duration = entry.Duration
                    });
                    artistEntry.Albums.Add(albumEntry);

                    this.ValidArtists.Add(artistEntry);
                }
            }

            // Log Events (TODO: Remove these, or change this library into an IEnumerable<LibraryEntry>)
            entry.LogEvent += OnEntryLog;
        }

        private bool ValidateEntry(LibraryEntry entry)
        {
            return entry != null &&
                   !string.IsNullOrEmpty(entry.FileName) &&
                   File.Exists(entry.FileName) &&
                   !string.IsNullOrEmpty(entry.Album) &&
                   entry.AlbumArtists.Any() &&
                   entry.AlbumArtists.All(x => !string.IsNullOrEmpty(x.Name)) &&
                   !string.IsNullOrEmpty(entry.AlbumArtists.First().Name);
        }

        private void OnEntryLog(string item1, LogMessageSeverity item2)
        {
            if (this.LogEvent != null)
                this.LogEvent(item1, LogMessageType.MusicBrainz, item2);
        }
    }
}
