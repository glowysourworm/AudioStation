using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent.Output.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderImportOutputViewModel : ViewModelBase, ILibraryLoaderImportOutput
    {
        string _destinationFolderBase;
        string _destinationPathCalculated;

        ObservableCollection<ILogMessage> _logMessages;
        ObservableCollection<IAcoustIDLookupResult> _acoustIDResults;
        ObservableCollection<ITagSmall> _musicBrainzRecordingMatches;

        int _tagSmallId;
        int _tagSmallFileReferenceMapId;
        int _tagSmallVendorMapId;
        int _fileReferenceId;
        int _genreId;
        int _artistId;
        int _albumId;
        int _trackId;
        int _trackGenreMapId;
        int _trackArtistMapId;

        //MusicBrainzPicture? _bestFrontCover;
        //MusicBrainzPicture? _bestBackCover;

        bool _acoustIDSuccess;
        bool _musicBrainzBasicSuccess;
        bool _musicBrainzArtworkSuccess;
        bool _tagEmbeddingSuccess;
        bool _fileMoveSuccess;
        bool _fileConversionSuccess;

        public string DestinationFolderBase
        {
            get { return _destinationFolderBase; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolderBase, value); }
        }
        public string DestinationPathCalculated
        {
            get { return _destinationPathCalculated; }
            set { this.RaiseAndSetIfChanged(ref _destinationPathCalculated, value); }
        }
        public ObservableCollection<ILogMessage> LogMessages
        {
            get { return _logMessages; }
            set { this.RaiseAndSetIfChanged(ref _logMessages, value); }
        }
        public ObservableCollection<IAcoustIDLookupResult> AcoustIDResults
        {
            get { return _acoustIDResults; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDResults, value); }
        }
        public ObservableCollection<ITagSmall> MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzRecordingMatches, value); }
        }
        public int TagSmallId
        {
            get { return _tagSmallId; }
            set { this.RaiseAndSetIfChanged(ref _tagSmallId, value); }
        }
        public int TagSmallFileReferenceMapId
        {
            get { return _tagSmallFileReferenceMapId; }
            set { this.RaiseAndSetIfChanged(ref _tagSmallFileReferenceMapId, value); }
        }
        public int TagSmallVendorMapId
        {
            get { return _tagSmallVendorMapId; }
            set { this.RaiseAndSetIfChanged(ref _tagSmallVendorMapId, value); }
        }
        public int FileReferenceId
        {
            get { return _fileReferenceId; }
            set { this.RaiseAndSetIfChanged(ref _fileReferenceId, value); }
        }
        public int GenreId
        {
            get { return _genreId; }
            set { this.RaiseAndSetIfChanged(ref _genreId, value); }
        }
        public int ArtistId
        {
            get { return _artistId; }
            set { this.RaiseAndSetIfChanged(ref _artistId, value); }
        }
        public int AlbumId
        {
            get { return _albumId; }
            set { this.RaiseAndSetIfChanged(ref _albumId, value); }
        }
        public int TrackId
        {
            get { return _trackId; }
            set { this.RaiseAndSetIfChanged(ref _trackId, value); }
        }
        public int TrackGenreMapId
        {
            get { return _trackGenreMapId; }
            set { this.RaiseAndSetIfChanged(ref _trackGenreMapId, value); }
        }
        public int TrackArtistMapId
        {
            get { return _trackArtistMapId; }
            set { this.RaiseAndSetIfChanged(ref _trackArtistMapId, value); }
        }
        public bool AcoustIDSuccess
        {
            get { return _acoustIDSuccess; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDSuccess, value); }
        }
        public bool MusicBrainzBasicSuccess
        {
            get { return _musicBrainzBasicSuccess; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzBasicSuccess, value); }
        }
        public bool MusicBrainzArtworkSuccess
        {
            get { return _musicBrainzArtworkSuccess; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzArtworkSuccess, value); }
        }
        public bool TagEmbeddingSuccess
        {
            get { return _tagEmbeddingSuccess; }
            set { this.RaiseAndSetIfChanged(ref _tagEmbeddingSuccess, value); }
        }
        public bool FileMoveSuccess
        {
            get { return _fileMoveSuccess; }
            set { this.RaiseAndSetIfChanged(ref _fileMoveSuccess, value); }
        }
        public bool FileConversionSuccess
        {
            get { return _fileConversionSuccess; }
            set { this.RaiseAndSetIfChanged(ref _fileConversionSuccess, value); }
        }

        IEnumerable<ILogMessage> ILibraryLoaderImportOutput.LogMessages
        {
            get { return _logMessages; }
            set
            {
                throw new NotSupportedException("Must not allow cast-setting of LogMessages collection");
            }
        }
        IEnumerable<IAcoustIDLookupResult> ILibraryLoaderImportOutput.AcoustIDResults
        {
            get { return _acoustIDResults; }
            set
            {
                throw new NotSupportedException("Must not allow cast-setting of AcoustIDResults collection");
            }
        }
        IEnumerable<ITagSmall> ILibraryLoaderImportOutput.MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set
            {
                throw new NotSupportedException("Must not allow cast-setting of MusicBrainzRecordingMatches collection");
            }
        }

        public LibraryLoaderImportOutputViewModel()
        {
            this.AcoustIDResults = new ObservableCollection<IAcoustIDLookupResult>();
            this.LogMessages = new ObservableCollection<ILogMessage>();
            this.MusicBrainzRecordingMatches = new ObservableCollection<ITagSmall>();
        }
    }
}
