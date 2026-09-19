using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core.Utility
{
    /// <summary>
    /// Static routines for mapping to/from the ATL library
    /// </summary>
    public static class TagMapper
    {
        /// <summary>
        /// Maps ITagFull to a TagSmall instance. The ID must be set in order to insert this 
        /// into the database or to update a databse record
        /// </summary>
        /// <param name="id">(required) for database use</param>
        /// <param name="tag">Data record</param>
        /// <returns>A new TagSmall instance (not linked to database)</returns>
        public static TagSmall MapDatabase(int id, ITagFull tag)
        {
            return new TagSmall()
            {
                Id = id,

                Album = tag.Album,
                AlbumArtist = tag.AlbumArtist,
                DurationMilliseconds = tag.DurationMilliseconds,
                MediaFormat = tag.MediaFormat,
                MediaNumber = tag.MediaNumber,
                MediaTotal = tag.MediaTotal,
                Year = tag.Year,
                Genre = tag.Genre,
                Title = tag.Title,
                TrackNumber = tag.TrackNumber,
                TrackTotal = tag.TrackTotal
            };
        }

        /// <summary>
        /// Maps ITagFull to a TagSmall instance. The ID must be set in order to insert this 
        /// into the database or to update a databse record
        /// </summary>
        /// <param name="id">(required) for database use</param>
        /// <param name="tag">Data record</param>
        /// <returns>A new TagSmall instance (not linked to database)</returns>
        public static TagSmall Map(ITagFull tag)
        {
            return new TagSmall()
            {
                Album = tag.Album,
                AlbumArtist = tag.AlbumArtist,
                DurationMilliseconds = tag.DurationMilliseconds,
                MediaFormat = tag.MediaFormat,
                MediaNumber = tag.MediaNumber,
                MediaTotal = tag.MediaTotal,
                Year = tag.Year,
                Genre = tag.Genre,
                Title = tag.Title,
                TrackNumber = tag.TrackNumber,
                TrackTotal = tag.TrackTotal
            };
        }

        /// <summary>
        /// Maps ITagSmall to new TagFull instance (these are related only to file tag data, not the database)
        /// </summary>
        public static TagFull Map(ITagSmall tag)
        {
            return new TagFull()
            {
                Album = tag.Album,
                AlbumArtist = tag.AlbumArtist,
                MediaNumber = tag.MediaNumber,
                MediaTotal = tag.MediaTotal,
                Genre = tag.Genre,
                TrackNumber = tag.TrackNumber,
                TrackTotal = tag.TrackTotal,
                MediaFormat = tag.MediaFormat,
                Year = tag.Year,
                DurationMilliseconds = tag.DurationMilliseconds
            };
        }
    }
}
