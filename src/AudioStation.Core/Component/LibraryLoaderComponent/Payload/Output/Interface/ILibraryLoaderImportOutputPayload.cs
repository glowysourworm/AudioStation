using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output.Interface
{
    public interface ILibraryLoaderImportOutputPayload
    {
        public string DestinationFolderBase { get; set; }
        public string DestinationPathCalculated { get; set; }
        public IEnumerable<ILogMessage> LogMessages { get; set; }
        public IEnumerable<IAcoustIDLookupResult> AcoustIDResults { get; set; }
        public IEnumerable<ITagSmall> MusicBrainzRecordingMatches { get; set; }
        public IEnumerable<MusicBrainzAcoustIDResult> MusicBrainzAcoustIDResults { get; set; }
        public Guid TagSmallId { get; set; }
        public Guid TagSmallFileReferenceMapId { get; set; }
        public Guid TagSmallVendorMapId { get; set; }
        public Guid FileReferenceId { get; set; }
        public Guid GenreId { get; set; }
        public Guid ArtistId { get; set; }
        public Guid AlbumId { get; set; }
        public Guid TrackId { get; set; }
        //public MusicBrainzPicture? BestFrontCover { get; set; }
        //public MusicBrainzPicture? BestBackCover { get; set; }
        public bool AcoustIDSuccess { get; set; }
        public bool MusicBrainzBasicSuccess { get; set; }
        public bool MusicBrainzArtworkSuccess { get; set; }
        public bool TagEmbeddingSuccess { get; set; }
        public bool FileMoveSuccess { get; set; }
        public bool FileConversionSuccess { get; set; }
        public LibraryWorkerResultLevel ImportResult { get; set; }
    }
}
