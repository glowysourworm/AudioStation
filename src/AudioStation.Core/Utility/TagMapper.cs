using ATL;

using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Utility
{
    /// <summary>
    /// Static routines for mapping to/from the ATL library
    /// </summary>
    public static class TagMapper
    {
        /// <summary>
        /// Maps our namespace onto the ATL namespace as directly as possible. The original file name corresponds to the Path
        /// property inside of ATL.Track; and must be set in the constructor because the SaveTo function does not allow you 
        /// to change the destination file (without Path being set); and I'd like to avoid ATL's copying of the file before
        /// setting it. So, we need to be in charge of our file movement on the UI side.
        /// </summary>
        public static Track MapTo(IAudioStationTag tag, string fileName)
        {
            // AudioStation -> ATL:  Some properties are ready only. All fields have been put in their proper place by
            //                       this point. (see IAudioStationTag)
            //
            var atlTrack = new Track(fileName);

            atlTrack.AdditionalFields = tag.AdditionalFields;
            atlTrack.Album = tag.Album;
            atlTrack.AlbumArtist = tag.AlbumArtist;
            atlTrack.Artist = tag.Artist;
            // Property: AudioFormat
            atlTrack.AudioSourceUrl = tag.AudioSourceUrl;
            // Property: BitDepth, Bitrate
            atlTrack.BPM = tag.BPM;
            atlTrack.CatalogNumber = tag.CatalogNumber;
            // Property: ChannelsArrangement
            // Property: Chapters
            // Property: ChaptersTableDescription
            // Property: CodecFamily
            atlTrack.Comment = tag.Comment;
            atlTrack.Composer = tag.Composer;
            atlTrack.Conductor = tag.Conductor;
            atlTrack.Copyright = tag.Copyright;
            atlTrack.Date = tag.Date;
            atlTrack.Description = tag.GeneralDescription;
            atlTrack.DiscNumber = tag.DiscNumber;
            atlTrack.DiscTotal = tag.DiscTotal;
            // Property: Duration
            // Property: DurationMs
            atlTrack.EmbeddedPictures.Clear();
            foreach (var picture in tag.EmbeddedPictures)
                atlTrack.EmbeddedPictures.Add(picture);

            atlTrack.EncodedBy = tag.EncodedBy;
            atlTrack.Encoder = tag.Encoder;
            atlTrack.Genre = tag.Genre;
            atlTrack.Group = tag.Group;
            atlTrack.InvolvedPeople = tag.InvolvedPeople;
            atlTrack.ISRC = tag.ISRC;
            atlTrack.Language = tag.Language;
            atlTrack.LongDescription = tag.LongDescription;
            atlTrack.Lyricist = tag.Lyricist;
            // Property: MetadataFormats
            atlTrack.OriginalAlbum = tag.OriginalAlbum;
            atlTrack.OriginalArtist = tag.OriginalArtist;
            atlTrack.OriginalReleaseDate = tag.OriginalReleaseDate;
            atlTrack.OriginalReleaseYear = tag.OriginalReleaseDate.Year;
            // Property: Path
            atlTrack.Popularity = tag.Popularity;
            atlTrack.ProductId = tag.ProductId;
            atlTrack.Publisher = tag.Publisher;
            atlTrack.PublishingDate = tag.PublishingDate;
            atlTrack.SeriesPart = tag.SeriesPart;
            atlTrack.SeriesTitle = tag.SeriesTitle;
            atlTrack.SortAlbum = tag.SortAlbum;
            atlTrack.SortAlbumArtist = tag.SortAlbumArtist;
            atlTrack.SortArtist = tag.SortArtist;
            atlTrack.SortTitle = tag.SortTitle;
            // Property: SupportedMetaDataFormats
            // Property: TechnicalInformation
            atlTrack.Title = tag.Title;
            atlTrack.TrackNumber = (int)tag.Track;
            atlTrack.TrackNumberStr = tag.TrackNumber;
            atlTrack.TrackTotal = tag.TrackTotal;
            atlTrack.Year = tag.Year;

            return atlTrack;
        }

        /// <summary>
        /// Maps the ATL tag onto our namespace as directly as possible
        /// </summary>
        public static AudioStationTag MapFrom(Track atlTrack)
        {
            // ATL -> AudioStation
            var tagFile = new AudioStationTag();

            tagFile.AdditionalFields = atlTrack.AdditionalFields;
            tagFile.Album = atlTrack.Album;
            tagFile.AlbumArtist = atlTrack.AlbumArtist;

            if (!string.IsNullOrEmpty(atlTrack.AlbumArtist)) 
                tagFile.AlbumArtists.Add(atlTrack.AlbumArtist);                         // NEEDS TO BE FINISHED

            tagFile.Artist = atlTrack.Artist;
            tagFile.AudioFormat = atlTrack.AudioFormat.Name;
            tagFile.AudioSourceUrl = atlTrack.AudioSourceUrl;
            tagFile.BitDepth = atlTrack.BitDepth;
            tagFile.BitRate = atlTrack.Bitrate;
            tagFile.BPM = atlTrack.BPM;
            tagFile.CatalogNumber = atlTrack.CatalogNumber;
            tagFile.Channels = atlTrack.ChannelsArrangement.NbChannels;
            tagFile.Chapters = atlTrack.Chapters;
            tagFile.ChaptersTableDescription = atlTrack.ChaptersTableDescription;
            tagFile.Comment = atlTrack.Comment;
            tagFile.Composer = atlTrack.Composer;
            tagFile.Conductor = atlTrack.Conductor;
            tagFile.Copyright = atlTrack.Copyright;
            tagFile.Date = atlTrack.Date ?? DateTime.MinValue;
            tagFile.DiscNumber = (ushort)(atlTrack.DiscNumber ?? 0);
            tagFile.DiscTotal = (ushort)(atlTrack.DiscTotal ?? 0);
            tagFile.Duration = TimeSpan.FromMilliseconds(atlTrack.DurationMs);
            tagFile.EmbeddedPictures = atlTrack.EmbeddedPictures;
            tagFile.EncodedBy = atlTrack.EncodedBy;
            tagFile.Encoder = atlTrack.Encoder;
            tagFile.GeneralDescription = atlTrack.Description;
            tagFile.Genre = atlTrack.Genre;

            if (!string.IsNullOrEmpty(tagFile.Genre))
                tagFile.Genres.Add(atlTrack.Genre);                                     // NEEDS TO BE FINISHED

            tagFile.Group = atlTrack.Group;
            tagFile.InvolvedPeople = atlTrack.InvolvedPeople;
            tagFile.IsDateYearOnly = false;
            tagFile.IsOriginalReleaseDateYearOnly = false;
            tagFile.ISRC = atlTrack.ISRC;
            tagFile.IsVariableBitRate = atlTrack.IsVBR;
            tagFile.Language = atlTrack.Language;
            tagFile.LongDescription = atlTrack.LongDescription;
            tagFile.Lyricist = atlTrack.Lyricist;
            tagFile.Lyrics = atlTrack.Lyrics;
            tagFile.MetadataFormats = atlTrack.MetadataFormats;
            tagFile.OriginalAlbum = atlTrack.OriginalAlbum;
            tagFile.OriginalArtist = atlTrack.OriginalArtist;
            tagFile.OriginalReleaseDate = atlTrack.OriginalReleaseDate ?? DateTime.MinValue;
            tagFile.Popularity = atlTrack.Popularity;
            tagFile.ProductId = atlTrack.ProductId;
            tagFile.Publisher = atlTrack.Publisher;
            tagFile.PublishingDate = atlTrack.PublishingDate ?? DateTime.MinValue;
            tagFile.SampleRate = atlTrack.SampleRate;
            tagFile.SeriesPart = atlTrack.SeriesPart;
            tagFile.SeriesTitle = atlTrack.SeriesTitle;
            tagFile.SortAlbum = atlTrack.SortAlbum;
            tagFile.SortAlbumArtist = atlTrack.SortAlbumArtist;
            tagFile.SortArtist = atlTrack.SortArtist;
            tagFile.SortTitle = atlTrack.SortTitle;
            tagFile.Title = atlTrack.Title;
            tagFile.Track = (uint)(atlTrack.TrackNumber ?? 0);
            tagFile.TrackNumber = atlTrack.TrackNumberStr;
            tagFile.TrackTotal = (ushort)(atlTrack.TrackTotal ?? 0);
            tagFile.Year = (int)(atlTrack.Year ?? 0);

            return tagFile;
        }
    }
}
