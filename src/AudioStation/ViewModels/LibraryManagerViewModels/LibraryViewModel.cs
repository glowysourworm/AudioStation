using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using AudioStation.Component;
using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.ViewModels.LibraryManagerViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly IViewModelLoader _viewModelLoader;
        private readonly int _libraryEntryPageSize = 100;

        ObservableCollection<LibraryEntryViewModel> _libraryEntries;
        ObservableCollection<LibraryEntryViewModel> _libraryEntryTabItems;
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
        LibraryManagerErrorFilterType _libraryManagerFilterType;

        int _libraryEntriesPageBeginEntryNumber;
        int _libraryEntriesPageEndEntryNumber;
        int _libraryEntryRequestPage;
        int _libraryEntryPage;

        SimpleCommand _libraryEntryPageRequestCommand;
        SimpleCommand<int> _libraryEntryPageRequestBackCommand;
        SimpleCommand<int> _libraryEntryPageRequestForwardCommand;
        SimpleCommand<LibraryEntryViewModel> _addLibraryEntryTabCommand;
        SimpleCommand<LibraryEntryViewModel> _removeLibraryEntryTabCommand;

        public ObservableCollection<LibraryEntryViewModel> LibraryEntries
        {
            get { return _libraryEntries; }
            set { RaiseAndSetIfChanged(ref _libraryEntries, value); }
        }
        public ObservableCollection<LibraryEntryViewModel> LibraryEntryTabItems
        {
            get { return _libraryEntryTabItems; }
            set { RaiseAndSetIfChanged(ref _libraryEntryTabItems, value); }
        }
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { RaiseAndSetIfChanged(ref _albums, value); }
        }
        public ObservableCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
            set { RaiseAndSetIfChanged(ref _artists, value); }
        }
        public ObservableCollection<GenreViewModel> Genres
        {
            get { return _genres; }
            set { RaiseAndSetIfChanged(ref _genres, value); }
        }

        public int TotalArtistCount
        {
            get { return _totalArtistCount; }
            set { RaiseAndSetIfChanged(ref _totalArtistCount, value); }
        }
        public int TotalAlbumCount
        {
            get { return _totalAlbumCount; }
            set { RaiseAndSetIfChanged(ref _totalAlbumCount, value); }
        }
        public int TotalLibraryEntriesCount
        {
            get { return _totalLibraryEntriesCount; }
            set { RaiseAndSetIfChanged(ref _totalLibraryEntriesCount, value); }
        }
        public int TotalGenresCount
        {
            get { return _totalGenresCount; }
            set { RaiseAndSetIfChanged(ref _totalGenresCount, value); }
        }
        public int TotalArtistFilteredCount
        {
            get { return _totalArtistFilteredCount; }
            set { RaiseAndSetIfChanged(ref _totalArtistFilteredCount, value); }
        }
        public int TotalAlbumFilteredCount
        {
            get { return _totalAlbumFilteredCount; }
            set { RaiseAndSetIfChanged(ref _totalAlbumFilteredCount, value); }
        }
        public int TotalLibraryEntriesFilteredCount
        {
            get { return _totalLibraryEntriesFilteredCount; }
            set { RaiseAndSetIfChanged(ref _totalLibraryEntriesFilteredCount, value); }
        }
        public int TotalGenresFilteredCount
        {
            get { return _totalGenresFilteredCount; }
            set { RaiseAndSetIfChanged(ref _totalGenresFilteredCount, value); }
        }

        public string ArtistSearch
        {
            get { return _artistSearch; }
            set { RaiseAndSetIfChanged(ref _artistSearch, value); }
        }
        public LibraryEntryViewModel LibraryEntrySearch
        {
            get { return _libraryEntrySearch; }
            set { RaiseAndSetIfChanged(ref _libraryEntrySearch, value); }
        }
        public LibraryManagerErrorFilterType LibraryManagerFilterType
        {
            get { return _libraryManagerFilterType; }
            set { RaiseAndSetIfChanged(ref _libraryManagerFilterType, value); }
        }

        public int LibraryEntriesPageBeginEntryNumber
        {
            get { return _libraryEntriesPageBeginEntryNumber; }
            set { RaiseAndSetIfChanged(ref _libraryEntriesPageBeginEntryNumber, value); }
        }
        public int LibraryEntriesPageEndEntryNumber
        {
            get { return _libraryEntriesPageEndEntryNumber; }
            set { RaiseAndSetIfChanged(ref _libraryEntriesPageEndEntryNumber, value); }
        }
        public int LibraryEntryRequestPage
        {
            get { return _libraryEntryRequestPage; }
            set { RaiseAndSetIfChanged(ref _libraryEntryRequestPage, value); }
        }
        public int LibraryEntryPage
        {
            get { return _libraryEntryPage; }
            set { RaiseAndSetIfChanged(ref _libraryEntryPage, value); }
        }

        public SimpleCommand LibraryEntryPageRequestCommand
        {
            get { return _libraryEntryPageRequestCommand; }
            set { RaiseAndSetIfChanged(ref _libraryEntryPageRequestCommand, value); }
        }
        public SimpleCommand<int> LibraryEntryPageRequestBackCommand
        {
            get { return _libraryEntryPageRequestBackCommand; }
            set { RaiseAndSetIfChanged(ref _libraryEntryPageRequestBackCommand, value); }
        }
        public SimpleCommand<int> LibraryEntryPageRequestForwardCommand
        {
            get { return _libraryEntryPageRequestForwardCommand; }
            set { RaiseAndSetIfChanged(ref _libraryEntryPageRequestForwardCommand, value); }
        }
        public SimpleCommand<LibraryEntryViewModel> AddLibraryEntryTabCommand
        {
            get { return _addLibraryEntryTabCommand; }
            set { RaiseAndSetIfChanged(ref _addLibraryEntryTabCommand, value); }
        }
        public SimpleCommand<LibraryEntryViewModel> RemoveLibraryEntryTabCommand
        {
            get { return _removeLibraryEntryTabCommand; }
            set { RaiseAndSetIfChanged(ref _removeLibraryEntryTabCommand, value); }
        }

        /// <summary>
        /// This instance should be owned by the LibraryManagerViewModel. The primary view model (main) 
        /// will have the manager view model injected (as a pattern).
        /// </summary>
        public LibraryViewModel(IViewModelLoader viewModelLoader)
        {
            _viewModelLoader = viewModelLoader;

            this.LibraryEntries = new ObservableCollection<LibraryEntryViewModel>();
            this.LibraryEntryTabItems = new ObservableCollection<LibraryEntryViewModel>();
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.Artists = new ObservableCollection<ArtistViewModel>();
            this.Genres = new ObservableCollection<GenreViewModel>();

            this.LibraryEntrySearch = new LibraryEntryViewModel(-1);

            // Library Entry Tabs (closeable / ManagerView)
            this.AddLibraryEntryTabCommand = new SimpleCommand<LibraryEntryViewModel>(viewModel =>
            {
                this.LibraryEntryTabItems.Add(viewModel);
            });
            this.RemoveLibraryEntryTabCommand = new SimpleCommand<LibraryEntryViewModel>(viewModel =>
            {
                this.LibraryEntryTabItems.Remove(viewModel);
            });

            // Manager Grid (pager)
            this.LibraryEntryPageRequestCommand = new SimpleCommand(() =>
            {
                ExecuteSearch(this.LibraryEntryRequestPage);
            });
            this.LibraryEntryPageRequestForwardCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.LibraryEntryPage + pageCount);

                ExecuteSearch(pageNumber);
            });
            this.LibraryEntryPageRequestBackCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.LibraryEntryPage - pageCount);

                ExecuteSearch(pageNumber);
            });

            // Listen to property changes for executing searches on the data grid
            this.LibraryEntrySearch.PropertyChanged += (sender, args) =>
            {
                this.LibraryEntryRequestPage = 1;

                ExecuteSearch(1);
            };
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

            this.Genres.AddRange(result.Results.Select(genre => new GenreViewModel(genre.Id)
            {
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
            this.LibraryEntriesPageEndEntryNumber = result.PageNumber * result.PageSize;
            this.TotalLibraryEntriesCount = result.TotalRecordCount;
            this.TotalLibraryEntriesFilteredCount = result.TotalRecordCountFiltered;
        }

        private void ExecuteSearch(int pageNumber)
        {
            PageResult<LibraryEntryViewModel> result;

            if (this.LibraryManagerFilterType == LibraryManagerErrorFilterType.None)
            {
                result = _viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = Math.Max(pageNumber, 0),
                    PageSize = _libraryEntryPageSize,
                    WhereCallback = (entity) => { return FilterEntityFields(entity); } 
                });
            }
            else
            {
                result = _viewModelLoader.LoadEntryPage(new PageRequest<Mp3FileReference, int>()
                {
                    PageNumber = Math.Max(pageNumber, 0),
                    PageSize = _libraryEntryPageSize,
                    WhereCallback = (entity) => { return FilterEntityFields(entity) && FilterFileErrors(entity); }
                });
            }
            
            LoadEntryPage(result, true);
        }

        private bool FilterFileErrors(Mp3FileReference entity)
        {
            switch (this.LibraryManagerFilterType)
            {
                case LibraryManagerErrorFilterType.None:
                    return true;
                case LibraryManagerErrorFilterType.FileLoadError:
                    return entity.IsFileLoadError;
                case LibraryManagerErrorFilterType.FileUnavailable:
                    return !entity.IsFileAvailable;
                case LibraryManagerErrorFilterType.FilePossiblyCorrupt:
                    return entity.IsFileCorrupt;
                default:
                    throw new Exception("Unhandled LibraryManagerErrorFilterType:  LibraryViewModel.cs");
            }
        }

        // Not likely to get any optimization for this call from postgres / EF
        private bool FilterEntityFields(Mp3FileReference entity)
        {
            var result = true;

            // If there are search settings, then demand that they're honored
            //
            if (this.LibraryEntrySearch.Album != string.Empty)
                result &= entity.Album?.Name?.Contains(this.LibraryEntrySearch.Album, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.Disc > 0)
                result &= entity.Album?.DiscNumber == this.LibraryEntrySearch.Disc;

            if (result && this.LibraryEntrySearch.FileCorruptMessage != string.Empty)
                result &= entity.FileCorruptMessage?.Contains(this.LibraryEntrySearch.FileCorruptMessage, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.FileLoadErrorMessage != string.Empty)
                result &= entity.FileErrorMessage?.Contains(this.LibraryEntrySearch.FileLoadErrorMessage, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.FileName != string.Empty)
                result &= entity.FileName?.Contains(this.LibraryEntrySearch.FileName) ?? false;

            if (result && this.LibraryEntrySearch.Id > 0)
                result &= entity.Id.ToString().Contains(this.LibraryEntrySearch.Id.ToString());

            if (result && this.LibraryEntrySearch.PrimaryArtist != string.Empty)
                result &= entity.PrimaryArtist?.Name?.Contains(this.LibraryEntrySearch.PrimaryArtist, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.PrimaryGenre != string.Empty)
                result &= entity.PrimaryGenre?.Name?.Contains(this.LibraryEntrySearch.PrimaryGenre, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.Title != string.Empty)
                result &= entity.Title?.Contains(this.LibraryEntrySearch.Title, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.LibraryEntrySearch.Track > 0)
                result &= entity.Track == this.LibraryEntrySearch.Track;

            return result;
        }
    }
}
