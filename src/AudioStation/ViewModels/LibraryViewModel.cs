using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.ViewModel.LibraryViewModels;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryViewModel : ViewModelBase
    {
        PagedObservableCollection<LibraryEntryViewModel> _libraryEntries;
        PagedObservableCollection<AlbumViewModel> _albums;
        PagedObservableCollection<ArtistViewModel> _artists;
        PagedObservableCollection<GenreViewModel> _genres;

        int _totalArtistCount;
        int _totalAlbumCount;
        int _totalLibraryEntriesCount;
        int _totalGenresCount;

        int _totalArtistFilteredCount;
        int _totalAlbumFilteredCount;
        int _totalLibraryEntriesFilteredCount;
        int _totalGenresFilteredCount;

        string _artistSearch;

        public PagedObservableCollection<LibraryEntryViewModel> LibraryEntries
        {
            get { return _libraryEntries; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntries, value); }
        }
        public PagedObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }
        public PagedObservableCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
            set { this.RaiseAndSetIfChanged(ref _artists, value); }
        }
        public PagedObservableCollection<GenreViewModel> Genres
        {
            get { return _genres; }
            set { this.RaiseAndSetIfChanged(ref _genres, value); }
        }

        public int TotalArtistCount
        {
            get { return _totalArtistCount; }
            set { this.RaiseAndSetIfChanged(ref _totalArtistCount, value); }
        }
        public int TotalAlbumCount
        {
            get { return _totalAlbumCount; }
            set { this.RaiseAndSetIfChanged(ref _totalAlbumCount, value); }
        }
        public int TotalLibraryEntriesCount
        {
            get { return _totalLibraryEntriesCount; }
            set { this.RaiseAndSetIfChanged(ref _totalLibraryEntriesCount, value); }
        }
        public int TotalGenresCount
        {
            get { return _totalGenresCount; }
            set { this.RaiseAndSetIfChanged(ref _totalGenresCount, value); }
        }
        public int TotalArtistFilteredCount
        {
            get { return _totalArtistFilteredCount; }
            set { this.RaiseAndSetIfChanged(ref _totalArtistFilteredCount, value); }
        }
        public int TotalAlbumFilteredCount
        {
            get { return _totalAlbumFilteredCount; }
            set { this.RaiseAndSetIfChanged(ref _totalAlbumFilteredCount, value); }
        }
        public int TotalLibraryEntriesFilteredCount
        {
            get { return _totalLibraryEntriesFilteredCount; }
            set { this.RaiseAndSetIfChanged(ref _totalLibraryEntriesFilteredCount, value); }
        }
        public int TotalGenresFilteredCount
        {
            get { return _totalGenresFilteredCount; }
            set { this.RaiseAndSetIfChanged(ref _totalGenresFilteredCount, value); }
        }

        public string ArtistSearch
        {
            get { return _artistSearch; }
            set { this.RaiseAndSetIfChanged(ref _artistSearch, value); }
        }

        [IocImportingConstructor]
        public LibraryViewModel(IModelController modelController)
        {
            this.LibraryEntries = new PagedObservableCollection<LibraryEntryViewModel>(100);
            this.Albums = new PagedObservableCollection<AlbumViewModel>(100);
            this.Artists = new PagedObservableCollection<ArtistViewModel>(100);
            this.Genres = new PagedObservableCollection<GenreViewModel>(100);
        }

        public void LoadArtists(PageResult<Mp3FileReferenceArtist> result, bool reset)
        {
            if (reset)
                this.Artists.Clear();

            this.Artists.AddRange(result.Results.Select(artist => new ArtistViewModel(artist.Id)
            {
                Artist = artist.Name
            }));

            this.TotalArtistCount = result.TotalRecordCount;
            this.TotalArtistFilteredCount = result.TotalRecordCountFiltered;
        }
        public void LoadAlbums(PageResult<Mp3FileReferenceAlbum> result, bool reset)
        {
            if (reset)
                this.Albums.Clear();

            this.Albums.AddRange(result.Results.Select(album => new AlbumViewModel(album.Id)
            {
                Album = album.Name
            }));

            this.TotalAlbumCount = result.TotalRecordCount;
            this.TotalAlbumFilteredCount = result.TotalRecordCountFiltered;
        }
        public void LoadGenres(PageResult<Mp3FileReferenceGenre> result, bool reset)
        {
            if (reset)
                this.Genres.Clear();

            this.Genres.AddRange(result.Results.Select(genre => new GenreViewModel()
            {
                Id = genre.Id,
                Name = genre.Name
            }));

            this.TotalGenresCount = result.TotalRecordCount;
            this.TotalGenresFilteredCount = result.TotalRecordCountFiltered;
        }
        public void LoadEntryPage(PageResult<Mp3FileReference> result, bool reset)
        {
            if (reset)
                this.LibraryEntries.Clear();

            this.LibraryEntries.AddRange(result.Results.Select(entry => new LibraryEntryViewModel()
            {
                Album = entry.Album?.Name ?? "Unknown",
                Id = entry.Id,
                Disc = 0,
                FileName = entry.FileName,
                PrimaryArtist = entry.PrimaryArtist?.Name ?? "Unknown",
                Title = entry.Title ?? "Unknown",
                Track = (uint)(entry.Track ?? 0)
            }));

            this.TotalLibraryEntriesCount = result.TotalRecordCount;
            this.TotalLibraryEntriesFilteredCount = result.TotalRecordCountFiltered;
        }
    }
}
