using AudioStation.Core.Component.LibraryLoaderComponent.Output.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Output
{
    public class LibraryLoaderImportOutput : ILibraryLoaderImportOutput
    {
        public string DestinationFolderBase { get; set; }
        public string DestinationPathCalculated { get; set; }
        public IEnumerable<ILogMessage> LogMessages { get; set; }
        public IEnumerable<IAcoustIDLookupResult> AcoustIDResults { get; set; }
        public IEnumerable<ITagSmall> MusicBrainzRecordingMatches { get; set; }
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
    }
}
