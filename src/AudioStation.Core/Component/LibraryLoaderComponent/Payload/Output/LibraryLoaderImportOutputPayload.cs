using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output
{
    public class LibraryLoaderImportOutputPayload : ILibraryLoaderImportOutputPayload
    {
        List<ILogMessage> _logMessages;
        List<IAcoustIDLookupResult> _acoustIDResults;
        List<ITagSmall> _musicBrainzRecordingMatches;
        List<MusicBrainzAcoustIDResult> _musicBrainzAcoustIDResults;

        public string DestinationFolderBase { get; set; }
        public string DestinationPathCalculated { get; set; }
        public IEnumerable<ILogMessage> LogMessages
        {
            get { return _logMessages; }
            set { _logMessages = new List<ILogMessage>(value); }
        }
        public IEnumerable<IAcoustIDLookupResult> AcoustIDResults
        {
            get { return _acoustIDResults; }
            set { _acoustIDResults = new List<IAcoustIDLookupResult>(value); }
        }
        public IEnumerable<ITagSmall> MusicBrainzRecordingMatches
        {
            get { return _musicBrainzRecordingMatches; }
            set { _musicBrainzRecordingMatches = new List<ITagSmall>(value); }
        }
        public IEnumerable<MusicBrainzAcoustIDResult> MusicBrainzAcoustIDResults
        {
            get { return _musicBrainzAcoustIDResults; }
            set { _musicBrainzAcoustIDResults = new List<MusicBrainzAcoustIDResult>(value); }
        }
        public int TagSmallId { get; set; }
        public int TagSmallFileReferenceMapId { get; set; }
        public int TagSmallVendorMapId { get; set; }
        public int FileReferenceId { get; set; }
        public int GenreId { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public int TrackId { get; set; }
        public int TrackGenreMapId { get; set; }
        public int TrackArtistMapId { get; set; }
        public bool AcoustIDSuccess { get; set; }
        public bool MusicBrainzBasicSuccess { get; set; }
        public bool MusicBrainzArtworkSuccess { get; set; }
        public bool TagEmbeddingSuccess { get; set; }
        public bool FileMoveSuccess { get; set; }
        public bool FileConversionSuccess { get; set; }
        public LibraryWorkerResultLevel ImportResult { get; set; }

        public LibraryLoaderImportOutputPayload()
        {
            _logMessages = new List<ILogMessage>();
            _acoustIDResults = new List<IAcoustIDLookupResult>();
            _musicBrainzRecordingMatches = new List<ITagSmall>();
            _musicBrainzAcoustIDResults = new List<MusicBrainzAcoustIDResult>();
        }
    }
}
