namespace AudioStation.Core.Model.Interface
{
    public interface ITagSmall
    {
        Guid Id { get; set; }

        [LibraryFormat(Name = "Artist", Description = "Artist of the track", TokenName = "artist", Use = LibraryFormatUse.File | LibraryFormatUse.Folder)]
        public string? AlbumArtist { get; set; }

        [LibraryFormat(Name = "Album", Description = "Album of the track", TokenName = "album", Use = LibraryFormatUse.File | LibraryFormatUse.Folder)]
        public string? Album { get; set; }

        [LibraryFormat(Name = "Title", Description = "Title of the track", TokenName = "title", Use = LibraryFormatUse.File)]
        public string? Title { get; set; }

        [LibraryFormat(Name = "Genre", Description = "Genre of the track", TokenName = "genre", Use = LibraryFormatUse.File | LibraryFormatUse.Folder)]
        public string? Genre { get; set; }

        [LibraryFormat(Name = "Track Number", Description = "Track number (or position) of the track in the track's release (or album)", TokenName = "track-number", Use = LibraryFormatUse.File)]
        public int? TrackNumber { get; set; }

        [LibraryFormat(Name = "Track Total", Description = "Number of tracks in the release (or album)", TokenName = "track-total", Use = LibraryFormatUse.File)]
        public int? TrackTotal { get; set; }

        [LibraryFormat(Name = "Media Number", Description = "Media (CD, Vinyl, ..) number (or position) for the release", TokenName = "media-number", Use = LibraryFormatUse.File)]
        public int? MediaNumber { get; set; }

        [LibraryFormat(Name = "Media Total", Description = "Media (CD, Vinyl, ..) total for the release (Disc 1 of 4)", TokenName = "media-total", Use = LibraryFormatUse.File)]
        public int? MediaTotal { get; set; }

        [LibraryFormat(Name = "Media Format", Description = "Type of media used for the release (e.g. CD, Vinyl, Cassette, ..)", TokenName = "media-format", Use = LibraryFormatUse.File | LibraryFormatUse.Folder)]
        public string? MediaFormat { get; set; }

        public int? DurationMilliseconds { get; set; }
        public int? Year { get; set; }
    }
}
