using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input
{
    public class LibraryLoaderImportPayload : ILibraryLoaderImportPayload
    {
        public LibraryImportType ImportType { get; set; }
        public LibraryImportSource TagSourcePreference { get; set; }

        /// <summary>
        /// Final tag for import:  This will be the data source for all of the entities for
        /// the library:  Track; Album; Artist; Genre; etc... The Id may only be set for
        /// existing entities - which will be checked during import.
        /// </summary>
        public TagSmall TagFinal { get; set; }

        /// <summary>
        /// Source vendor for the tag data
        /// </summary>
        public VendorNames TagSourceVendor { get; set; }

        /// <summary>
        /// RecordingId used to identify Music Brainz tag source data
        /// </summary>
        public Guid? MusicBrainzRecordingId { get; set; }

        public string SourceFullPath { get; set; }
        public string DestinationFolder { get; set; }

        public bool IsSourceDirectoryReadonly { get; set; }
        public bool EmbedImportTagData { get; set; }

        public TrackGroupingType GroupingType { get; set; }
        public TrackNamingType NamingType { get; set; }

        public string MigrationSourceDirectory { get; set; }
        public bool MigrationDeleteSourceFiles { get; set; }
        public bool MigrationDeleteSourceFolders { get; set; }
        public bool MigrationOverwriteDestinationFiles { get; set; }

        public bool ServiceIncludeAcoustID { get; set; }
        public bool ServiceIncludeMusicBrainzBasic { get; set; }
        public bool ServiceIncludeMusicBrainzArtwork { get; set; }

        public bool ServiceOverwriteAcoustID { get; set; }
        public bool ServiceOverwriteMusicBrainzBasic { get; set; }
        public bool ServiceOverwriteMusicBrainzArtwork { get; set; }

        public bool LibraryOverwriteExistingFiles { get; set; }
        public bool LibraryOverwriteExistingTracks { get; set; }
        public bool LibraryOverwriteExistingAlbums { get; set; }
        public bool LibraryOverwriteExistingArtists { get; set; }
        public bool LibraryOverwriteExistingGenres { get; set; }

        public AudioEncoderInfo ImportFormat { get; set; }
        public bool ConvertAudioFormat { get; set; }

        public LibraryLoaderImportPayload()
        {
            this.ImportFormat = new AudioEncoderInfo();
        }
    }
}
