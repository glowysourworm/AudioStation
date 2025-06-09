using System.Windows;
using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModels;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.LibraryViewModels.Comparer;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryViewModel : ViewModelBase
    {
        private readonly IDialogController _dialogController;
        private readonly IAudioController _audioController;
        private readonly IModelController _modelController;
        private readonly ILibraryLoader _libraryLoader;
        private readonly IOutputController _outputController;
        private readonly IIocEventAggregator _eventAggregator;

        SortedObservableCollection<LibraryEntryViewModel> _libraryEntries;

        SortedObservableCollection<AlbumViewModel> _albums;
        SortedObservableCollection<ArtistViewModel> _artists;
        SortedObservableCollection<GenreViewModel> _genres;

        public SortedObservableCollection<LibraryEntryViewModel> LibraryEntries
        {
            get { return _libraryEntries; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntries, value); }
        }
        public SortedObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }
        public SortedObservableCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
            set { this.RaiseAndSetIfChanged(ref _artists, value); }
        }
        public SortedObservableCollection<GenreViewModel> Genres
        {
            get { return _genres; }
            set { this.RaiseAndSetIfChanged(ref _genres, value); }
        }

        [IocImportingConstructor]
        public LibraryViewModel(ILibraryLoader libraryLoader,
                                IModelController modelController,
                                IIocEventAggregator eventAggregator,
                                IOutputController outputController)
        {
            _outputController = outputController;

            this.LibraryEntries = new SortedObservableCollection<LibraryEntryViewModel>(new PropertyComparer<string, LibraryEntryViewModel>(x => x.FileName));
            this.Albums = new SortedObservableCollection<AlbumViewModel>(new PropertyComparer<string, AlbumViewModel>(x => x.Album));
            this.Artists = new SortedObservableCollection<ArtistViewModel>(new PropertyComparer<string, ArtistViewModel>(x => x.Artist));
            this.Genres = new SortedObservableCollection<GenreViewModel>(new PropertyComparer<string, GenreViewModel>(x => x.Name));

            // Library Loader
            libraryLoader.LibraryEntryLoaded += OnLibraryEntryLoaded;

            // Model Controller (load the primary view model) (pre-initialized)
            foreach (var entry in modelController.Library.Entries)
            {
                this.LibraryEntries.Add(MapEntry(entry));
            }
            foreach (var entry in modelController.Library.Albums)
            {
                this.Albums.Add(MapAlbum(entry));
            }
            foreach (var entry in modelController.Library.Artists)
            {
                this.Artists.Add(MapArtist(entry));
            }
            foreach (var entry in modelController.Library.Genres)
            {
                this.Genres.Add(new GenreViewModel()
                {
                    Id = entry.Id,
                    Name = entry.Name
                });

            }
        }

        private void OnLibraryEntryLoaded(LibraryEntry entry)
        {
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnLibraryEntryLoaded, DispatcherPriority.ApplicationIdle, entry);
            else
            {
                if (!this.LibraryEntries.Any(item => item.FileName == entry.FileName))
                {
                    this.LibraryEntries.Add(MapEntry(entry));
                }
                else
                {
                    _outputController.AddLog("Library Entry already exists! {0}", LogMessageType.General, LogLevel.Error, entry.FileName);
                }
            }
        }

        private AlbumViewModel MapAlbum(Album album)
        {
            var result = new AlbumViewModel()
            {
                Id = album.Id,
                Album = album.Name,
                Duration = TimeSpan.Zero,       // TODO
                PrimaryArtist = album.Tracks.FirstOrDefault()?.PrimaryArtist ?? string.Empty,
                Year = album.Year
            };

            result.Tracks.AddRange(album.Tracks.Select(x => GetMappedEntry(x)));

            return result;
        }

        private ArtistViewModel MapArtist(Artist artist)
        {
            var result = new ArtistViewModel()
            {
                Id = artist.Id,
                Artist = artist.Name
            };

            result.Albums.AddRange(artist.Albums.Select(x => GetMappedAlbum(x)));

            return result;
        }

        private AlbumViewModel GetMappedAlbum(Album album)
        {
            return this.Albums.First(x => x.Id == album.Id);
        }
        private LibraryEntryViewModel GetMappedEntry(LibraryEntry entry)
        {
            return this.LibraryEntries.First(x => x.Id == entry.Id);
        }

        private LibraryEntryViewModel MapEntry(LibraryEntry entry)
        {
            return new LibraryEntryViewModel()
            {
                Album = entry.Album,
                Disc = entry.Disc,
                FileName = entry.FileName,
                Id = entry.Id,
                LoadError = entry.FileError,
                LoadErrorMessage = entry.FileErrorMessage,
                PrimaryArtist = entry.PrimaryArtist,
                PrimaryGenre = entry.PrimaryGenre,
                Title = entry.Title,
                Track = entry.Track
            };
        }
    }
}
