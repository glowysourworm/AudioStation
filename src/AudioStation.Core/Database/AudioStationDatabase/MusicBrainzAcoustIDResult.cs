namespace AudioStation.Core.Database.AudioStationDatabase
{
    public class MusicBrainzAcoustIDResult : AudioStationViewEntityBase
    {
        public int TagSmallId { get; set; }
        public Guid AcoustIDLookupId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public double? Score { get; set; }
        public string? FileName { get; set; }
        public string? Genre { get; set; }
        public string? AlbumArtist { get; set; }
        public string? Album { get; set; }
        public string? Title { get; set; }
        public int? TrackNumber { get; set; }
        public int? TrackTotal { get; set; }
        public int? MediaNumber { get; set; }
        public int? MediaTotal { get; set; }
        public string? MediaFormat { get; set; }
        public int? DurationMilliseconds { get; set; }
        public int? Year { get; set; }

        /// <summary>
        /// This method is needed for one or more UI components.
        /// </summary>
        public override string ToString()
        {
            var header = this.Title ?? this.Album ?? this.AlbumArtist ?? "(Not Set)";
            var footer = "[" + (this.TrackNumber ?? 0).ToString() + " (of) " + (this.TrackTotal ?? 0).ToString() + "]";

            if (this.Score != null)
                footer += string.Format(" ({0:P2})", this.Score);

            return header + " " + footer;
        }
    }
}
