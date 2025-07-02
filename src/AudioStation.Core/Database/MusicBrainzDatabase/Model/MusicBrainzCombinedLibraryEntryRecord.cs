namespace AudioStation.Core.Database.MusicBrainzDatabase.Model
{
    /// <summary>
    /// This is a combined record for taking most of the MusicBrainz data at once - PER TRACK (this should go
    /// into a tag, eventually, and anywhere else where we need combined information for the track)
    /// </summary>
    public class MusicBrainzCombinedLibraryEntryRecord
    {
        public MusicBrainzRecordingEntity Recording { get; set; }
        public MusicBrainzReleaseEntity Release { get; set; }
        public MusicBrainzMediumEntity Medium { get; set; }                     // The (SINGLE) medium (disc / vinyl / or other format...) for this track
        public MusicBrainzTrackEntity Track { get; set; }
        public List<MusicBrainzArtistEntity> Artists { get; set; }
        public List<MusicBrainzLabelEntity> Labels { get; set; }
        public List<MusicBrainzGenreEntity> RecordingGenres { get; set; }
        public List<MusicBrainzTagEntity> RecordingTags { get; set; }
        public List<MusicBrainzPicture> ReleasePictures { get; set; }

        public MusicBrainzCombinedLibraryEntryRecord()
        {
            this.Artists = new List<MusicBrainzArtistEntity>();
            this.Labels = new List<MusicBrainzLabelEntity>();
            this.RecordingGenres = new List<MusicBrainzGenreEntity>();
            this.RecordingTags = new List<MusicBrainzTagEntity>();
            this.ReleasePictures = new List<MusicBrainzPicture>();
        }
    }
}
