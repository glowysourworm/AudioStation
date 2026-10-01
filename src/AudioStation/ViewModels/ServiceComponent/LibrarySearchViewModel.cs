using System.ComponentModel;
using System.Windows.Data;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.Utilities;

namespace AudioStation.ViewModels.ServiceComponent
{
    /// <summary>
    /// Component used for searching the library and providing the data component LibraryViewModel (primary)
    /// </summary>
    public class LibrarySearchViewModel : ServiceComponentViewModelBase
    {
        LibraryViewModel _library;

        // Primary Search Collection
        ICollectionView _artistCollection;

        LibrarySearchType _searchType;
        string _searchText;

        public LibraryViewModel Library
        {
            get { return _library; }
            private set { this.RaiseAndSetIfChanged(ref _library, value); }
        }

        public ICollectionView SearchCollection
        {
            get
            {
                switch (_searchType)
                {
                    case LibrarySearchType.Artist:
                    case LibrarySearchType.Album:
                    case LibrarySearchType.Genre:
                    case LibrarySearchType.Track:
                        return _artistCollection;
                    default:
                        throw new Exception("Unhandled search type");
                }
            }
        }

        public LibrarySearchType SearchType
        {
            get { return _searchType; }
            set { this.RaiseAndSetIfChanged(ref _searchType, value); }
        }
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public LibrarySearchViewModel() : base("Library Search")
        {
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override bool CanLoad()
        {
            return !this.Loaded;
        }

        public override bool CanReset()
        {
            return this.Loaded;
        }

        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.Initialized = true;
        }

        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.Library = audioStationController.ComponentController.GetDataComponent<LibraryViewModel>();

            _artistCollection = CollectionViewSource.GetDefaultView(this.Library.Artists);

            switch (_searchType)
            {
                case LibrarySearchType.Artist:
                    _artistCollection.Filter = ArtistFilter;
                    break;
                case LibrarySearchType.Album:
                    _artistCollection.Filter = AlbumFilter;
                    break;
                case LibrarySearchType.Genre:
                    _artistCollection.Filter = GenreFilter;
                    break;
                case LibrarySearchType.Track:
                    _artistCollection.Filter = TrackFilter;
                    break;
                default:
                    throw new Exception("Unhandled library search type");
            }

            this.SearchCollection.Refresh();

            this.Loaded = true;
        }

        public override void Execute(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Library Search

            // CALL REFRESH ON ICollectionView (??)

            this.SearchCollection.Refresh();
        }

        public override void Reset(Guid? componentPartId, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            _artistCollection = null;

            this.Loaded = false;
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            // Search Collection Change
            if (name == nameof(SearchType))
            {
                // -> Reset
                if (CanReset())
                {
                    RaiseResetEvent();
                }

                // -> Load
                if (CanLoad())
                {
                    RaiseLoadEvent();
                }

                // -> Execute
                if (CanExecute())
                {
                    RaiseExecuteEvent();
                }

                OnPropertyChanged("SearchCollection");
            }

            else if (name == nameof(SearchText))
            {
                // -> Execute
                if (CanExecute())
                {
                    RaiseExecuteEvent();
                }

                OnPropertyChanged("SearchCollection");
            }
        }

        private bool AlbumFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return true;

            var viewModel = (ArtistViewModel)item;

            // Ignore Case
            return viewModel.Albums.Any(x => StringHelpers.ContainsIC(x.Album, _searchText));
        }
        private bool ArtistFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return true;

            var viewModel = (ArtistViewModel)item;

            // Ignore Case
            return StringHelpers.ContainsIC(viewModel.Artist, _searchText);
        }
        private bool GenreFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return true;

            var viewModel = (ArtistViewModel)item;

            // Ignore Case
            return viewModel.Albums
                            .SelectMany(x => x.Media)
                            .SelectMany(x => x.Tracks)
                            .Any(x => StringHelpers.ContainsIC(x.Genre, _searchText));
        }
        private bool TrackFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return true;

            var viewModel = (ArtistViewModel)item;

            // Ignore Case
            return viewModel.Albums
                            .SelectMany(x => x.Media)
                            .SelectMany(x => x.Tracks)
                            .Any(x => StringHelpers.ContainsIC(x.Title, _searchText));
        }
    }
}
