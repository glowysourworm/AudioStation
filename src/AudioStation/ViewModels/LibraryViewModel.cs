using System.Collections.ObjectModel;

using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels
{
    [IocExportDefault]
    public class LibraryViewModel : ViewModelBase
    {
        private readonly int _libraryEntryPageSize = 100;

        ObservableCollection<LibraryEntryViewModel> _libraryEntries;
        ObservableCollection<AlbumViewModel> _albums;
        ObservableCollection<ArtistViewModel> _artists;
        ObservableCollection<GenreViewModel> _genres;

        int _totalArtistCount;
        int _totalAlbumCount;
        int _totalLibraryEntriesCount;
        int _totalGenresCount;

        int _totalArtistFilteredCount;
        int _totalAlbumFilteredCount;
        int _totalLibraryEntriesFilteredCount;
        int _totalGenresFilteredCount;

        string _artistSearch;
        LibraryEntryViewModel _libraryEntrySearch;

        int _libraryEntriesPageBeginEntryNumber;
        int _libraryEntriesPageEndEntryNumber;
        int _libraryEntryRequestPage;
        int _libraryEntryPage;

        SimpleCommand _libraryEntryPageRequestCommand;
        SimpleCommand<int> _libraryEntryPageRequestBackCommand;
        SimpleCommand<int> _libraryEntryPageRequestForwardCommand;

        public ObservableCollection<LibraryEntryViewModel> LibraryEntries
        {
            get { return _libraryEntries; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntries, value); }
        }
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }
        public ObservableCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
            set { this.RaiseAndSetIfChanged(ref _artists, value); }
        }
        public ObservableCollection<GenreViewModel> Genres
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
        public LibraryEntryViewModel LibraryEntrySearch
        {
            get { return _libraryEntrySearch; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntrySearch, value); }
        }

        public int LibraryEntriesPageBeginEntryNumber
        {
            get { return _libraryEntriesPageBeginEntryNumber; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntriesPageBeginEntryNumber, value); }
        }
        public int LibraryEntriesPageEndEntryNumber
        {
            get { return _libraryEntriesPageEndEntryNumber; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntriesPageEndEntryNumber, value); }
        }
        public int LibraryEntryRequestPage
        {
            get { return _libraryEntryRequestPage; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntryRequestPage, value); }
        }
        public int LibraryEntryPage
        {
            get { return _libraryEntryPage; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntryPage, value); }
        }

        public SimpleCommand LibraryEntryPageRequestCommand
        {
            get { return _libraryEntryPageRequestCommand; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntryPageRequestCommand, value); }
        }
        public SimpleCommand<int> LibraryEntryPageRequestBackCommand
        {
            get { return _libraryEntryPageRequestBackCommand; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntryPageRequestBackCommand, value); }
        }
        public SimpleCommand<int> LibraryEntryPageRequestForwardCommand
        {
            get { return _libraryEntryPageRequestForwardCommand; }
            set { this.RaiseAndSetIfChanged(ref _libraryEntryPageRequestForwardCommand, value); }
        }

        [IocImportingConstructor]
        public LibraryViewModel(IViewModelLoader viewModelLoader)
        {
            this.LibraryEntries = new ObservableCollection<LibraryEntryViewModel>();
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.Artists = new ObservableCollection<ArtistViewModel>();
            this.Genres = new ObservableCollection<GenreViewModel>();

            this.LibraryEntrySearch = new LibraryEntryViewModel();

            this.LibraryEntryPageRequestCommand = new SimpleCommand(() =>
            {
                var result = viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = this.LibraryEntryRequestPage,
                    PageSize = _libraryEntryPageSize
                });

                this.LoadEntryPage(result, true);
            });
            this.LibraryEntryPageRequestForwardCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.LibraryEntryPage + pageCount);

                var result = viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = pageNumber,
                    PageSize = _libraryEntryPageSize
                });

                this.LoadEntryPage(result, true);
            });
            this.LibraryEntryPageRequestBackCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.LibraryEntryPage - pageCount);

                var result = viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = pageNumber,
                    PageSize = _libraryEntryPageSize
                });

                this.LoadEntryPage(result, true);
            });
        }

        public void LoadArtists(PageResult<ArtistViewModel> result, bool reset)
        {
            if (reset)
                this.Artists.Clear();

            this.Artists.AddRange(result.Results);

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
        public void LoadEntryPage(PageResult<LibraryEntryViewModel> result, bool reset)
        {
            if (reset)
                this.LibraryEntries.Clear();

            this.LibraryEntries.AddRange(result.Results);

            this.LibraryEntryPage = result.PageNumber;
            this.LibraryEntryRequestPage = result.PageNumber;
            this.LibraryEntriesPageBeginEntryNumber = (result.PageNumber - 1) * result.PageSize + 1;
            this.LibraryEntriesPageEndEntryNumber = (result.PageNumber) * result.PageSize;
            this.TotalLibraryEntriesCount = result.TotalRecordCount;
            this.TotalLibraryEntriesFilteredCount = result.TotalRecordCountFiltered;
        }
    }
}
