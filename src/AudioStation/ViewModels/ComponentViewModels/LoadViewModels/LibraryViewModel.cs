using System.Collections.ObjectModel;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

using SimpleWpf.Extensions.Command;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LoadViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly int _trackPageSize = 100;

        ObservableCollection<TrackViewModel> _tracks;
        ObservableCollection<TrackViewModel> _trackTabItems;
        ObservableCollection<AlbumViewModel> _albums;
        ObservableCollection<ArtistViewModel> _artists;
        ObservableCollection<ArtistViewModel> _artistsFull;
        ObservableCollection<GenreViewModel> _genres;

        int _totalArtistCount;
        int _totalAlbumCount;
        int _totalCount;
        int _totalGenresCount;

        int _totalArtistFilteredCount;
        int _totalAlbumFilteredCount;
        int _totalLibraryEntriesFilteredCount;
        int _totalGenresFilteredCount;

        string _artistSearch;
        TrackViewModel _trackSearch;
        LibraryManagerErrorFilterType _libraryManagerFilterType;

        int _trackPageBeginEntryNumber;
        int _trackPageEndEntryNumber;
        int _trackRequestPage;
        int _trackPage;

        SimpleCommand _trackPageRequestCommand;
        SimpleCommand<int> _trackPageRequestBackCommand;
        SimpleCommand<int> _trackPageRequestForwardCommand;
        SimpleCommand<TrackViewModel> _addTrackTabCommand;
        SimpleCommand<TrackViewModel> _removeTrackTabCommand;

        public ObservableCollection<TrackViewModel> Tracks
        {
            get { return _tracks; }
            set { RaiseAndSetIfChanged(ref _tracks, value); }
        }
        public ObservableCollection<TrackViewModel> TrackTabItems
        {
            get { return _trackTabItems; }
            set { RaiseAndSetIfChanged(ref _trackTabItems, value); }
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
        public int TotalTrackCount
        {
            get { return _totalCount; }
            set { RaiseAndSetIfChanged(ref _totalCount, value); }
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
        public int TotalTrackFilteredCount
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
            set { RaiseAndSetIfChanged(ref _artistSearch, value); ExecuteArtistSearch(); }
        }
        public TrackViewModel TrackSearch
        {
            get { return _trackSearch; }
            set { RaiseAndSetIfChanged(ref _trackSearch, value); }
        }
        public LibraryManagerErrorFilterType LibraryManagerFilterType
        {
            get { return _libraryManagerFilterType; }
            set { RaiseAndSetIfChanged(ref _libraryManagerFilterType, value); }
        }

        public int TrackPageBeginEntryNumber
        {
            get { return _trackPageBeginEntryNumber; }
            set { RaiseAndSetIfChanged(ref _trackPageBeginEntryNumber, value); }
        }
        public int TrackPageEndEntryNumber
        {
            get { return _trackPageEndEntryNumber; }
            set { RaiseAndSetIfChanged(ref _trackPageEndEntryNumber, value); }
        }
        public int TrackRequestPage
        {
            get { return _trackRequestPage; }
            set { RaiseAndSetIfChanged(ref _trackRequestPage, value); }
        }
        public int TrackPage
        {
            get { return _trackPage; }
            set { RaiseAndSetIfChanged(ref _trackPage, value); }
        }

        public SimpleCommand TrackPageRequestCommand
        {
            get { return _trackPageRequestCommand; }
            set { RaiseAndSetIfChanged(ref _trackPageRequestCommand, value); }
        }
        public SimpleCommand<int> TrackPageRequestBackCommand
        {
            get { return _trackPageRequestBackCommand; }
            set { RaiseAndSetIfChanged(ref _trackPageRequestBackCommand, value); }
        }
        public SimpleCommand<int> TrackPageRequestForwardCommand
        {
            get { return _trackPageRequestForwardCommand; }
            set { RaiseAndSetIfChanged(ref _trackPageRequestForwardCommand, value); }
        }
        public SimpleCommand<TrackViewModel> AddTrackTabCommand
        {
            get { return _addTrackTabCommand; }
            set { RaiseAndSetIfChanged(ref _addTrackTabCommand, value); }
        }
        public SimpleCommand<TrackViewModel> RemoveTrackTabCommand
        {
            get { return _removeTrackTabCommand; }
            set { RaiseAndSetIfChanged(ref _removeTrackTabCommand, value); }
        }

        /// <summary>
        /// This instance should be owned by the LibraryManagerViewModel. The primary view model (main) 
        /// will have the manager view model injected (as a pattern).
        /// </summary>
        public LibraryViewModel()
        {
            _artistsFull = new ObservableCollection<ArtistViewModel>();

            this.Tracks = new ObservableCollection<TrackViewModel>();
            this.TrackTabItems = new ObservableCollection<TrackViewModel>();
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.Artists = new ObservableCollection<ArtistViewModel>();
            this.Genres = new ObservableCollection<GenreViewModel>();

            this.TrackSearch = new TrackViewModel(-1);

            // Library Entry Tabs (closeable / ManagerView)
            this.AddTrackTabCommand = new SimpleCommand<TrackViewModel>(viewModel =>
            {
                this.TrackTabItems.Add(viewModel);
            });
            this.RemoveTrackTabCommand = new SimpleCommand<TrackViewModel>(viewModel =>
            {
                this.TrackTabItems.Remove(viewModel);
            });

            // Manager Grid (pager)
            this.TrackPageRequestCommand = new SimpleCommand(() =>
            {
                ExecuteSearch(this.TrackRequestPage);
            });
            this.TrackPageRequestForwardCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.TrackPage + pageCount);

                ExecuteSearch(pageNumber);
            });
            this.TrackPageRequestBackCommand = new SimpleCommand<int>((pageCount) =>
            {
                var pageNumber = Math.Max(1, this.TrackPage - pageCount);

                ExecuteSearch(pageNumber);
            });

            // Listen to property changes for executing searches on the data grid
            this.TrackSearch.PropertyChanged += (sender, args) =>
            {
                this.TrackRequestPage = 1;

                ExecuteSearch(1);
            };
        }

        private void ExecuteArtistSearch()
        {
            this.Artists.Clear();

            if (!string.IsNullOrWhiteSpace(this.ArtistSearch))
                this.Artists.AddRange(_artistsFull.Where(artist => artist.Artist.Contains(this.ArtistSearch)));

            else
                this.Artists.AddRange(_artistsFull);
        }

        public void LoadEntryPage(PageResult<TrackViewModel> result, bool reset)
        {
            if (reset)
                this.Tracks.Clear();

            this.Tracks.AddRange(result.Results);

            this.TrackPage = result.PageNumber;
            this.TrackRequestPage = result.PageNumber;
            this.TrackPageBeginEntryNumber = ((result.PageNumber - 1) * result.PageSize) + 1;
            this.TrackPageEndEntryNumber = result.PageNumber * result.PageSize;
            this.TotalTrackCount = result.TotalRecordCount;
            this.TotalTrackFilteredCount = result.TotalRecordCountFiltered;
        }

        private void ExecuteSearch(int pageNumber)
        {
            PageResult<TrackViewModel> result;

            if (this.LibraryManagerFilterType == LibraryManagerErrorFilterType.None)
            {
                //result = _viewModelLoader.LoadEntryPage(new PageRequest<Track, int>()
                //{
                //    PageNumber = Math.Max(pageNumber, 0),
                //    PageSize = _trackPageSize,
                //    WhereCallback = (entity) => { return FilterEntityFields(entity); }
                //});
            }
            else
            {
                //result = _viewModelLoader.LoadEntryPage(new PageRequest<Track, int>()
                //{
                //    PageNumber = Math.Max(pageNumber, 0),
                //    PageSize = _trackPageSize,
                //    WhereCallback = (entity) => { return FilterEntityFields(entity) && FilterFileErrors(entity); }
                //});
            }

            //LoadEntryPage(result, true);
        }

        private bool FilterFileErrors(Track entity)
        {
            switch (this.LibraryManagerFilterType)
            {
                case LibraryManagerErrorFilterType.None:
                    return true;
                case LibraryManagerErrorFilterType.FileLoadError:
                    return entity.FileReference.IsFileLoadError;
                case LibraryManagerErrorFilterType.FileUnavailable:
                    return !entity.FileReference.IsFileAvailable;
                default:
                    throw new Exception("Unhandled LibraryManagerErrorFilterType:  LibraryViewModel.cs");
            }
        }

        // Not likely to get any optimization for this call from postgres / EF
        private bool FilterEntityFields(Track entity)
        {
            var result = true;

            // If there are search settings, then demand that they're honored
            //
            if (this.TrackSearch.Album != string.Empty)
                result &= entity.Album?.Name?.Contains(this.TrackSearch.Album, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.Disc > 0)
                result &= entity.Album?.MediaNumber == this.TrackSearch.Disc;

            if (result && this.TrackSearch.FileCorruptMessage != string.Empty)
                result &= entity.FileReference.FileCorruptMessage?.Contains(this.TrackSearch.FileCorruptMessage, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.FileLoadErrorMessage != string.Empty)
                result &= entity.FileReference.FileErrorMessage?.Contains(this.TrackSearch.FileLoadErrorMessage, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.FileName != string.Empty)
                result &= entity.FileReference.FileName?.Contains(this.TrackSearch.FileName) ?? false;

            if (result && this.TrackSearch.Id > 0)
                result &= entity.Id.ToString().Contains(this.TrackSearch.Id.ToString());

            if (result && this.TrackSearch.PrimaryArtist != string.Empty)
                result &= entity.PrimaryArtist?.Name?.Contains(this.TrackSearch.PrimaryArtist, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.PrimaryGenre != string.Empty)
                result &= entity.PrimaryGenre?.Name?.Contains(this.TrackSearch.PrimaryGenre, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.Title != string.Empty)
                result &= entity.Title?.Contains(this.TrackSearch.Title, StringComparison.OrdinalIgnoreCase) ?? false;

            if (result && this.TrackSearch.Track > 0)
                result &= entity.Number == this.TrackSearch.Track;

            return result;
        }
    }
}
