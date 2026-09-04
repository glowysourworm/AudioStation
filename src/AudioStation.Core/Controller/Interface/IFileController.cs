using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Controller.Interface
{
    public interface IFileController : IAudioStationService
    {
        public enum StorageType
        {
            /// <summary>
            /// File is put into a temporary cache (see Configuration) in a single flat folder
            /// </summary>
            DiskCache,

            /// <summary>
            /// File is put into its final location based on configuration (e.g. Album/Artist/front-cover.bmp)
            /// </summary>
            DiskPermanent
        }

        /// <summary>
        /// Gets bitmap data for a previously stored file
        /// </summary>
        /// <param name="filePath">This would be returned by the IFileController to store in the database FileReference table</param>
        /// <returns>Bitmap image data for the image</returns>
        BitmapImageData GetImage(string filePath);

        /// <summary>
        /// Save audio file to music (or) audio books library permanent storage
        /// </summary>
        /// <param name="stagedFilePath">File full path of staged file (this should be already validated for migration)</param>
        /// <param name="trackType">Type of audio track (music, audiobook, ...)</param>
        /// <param name="track">Track title for the audio track</param>
        /// <param name="trackCount">Track count for the album</param>
        /// <param name="trackNumber">Track number for the track</param>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <param name="overwrite">Option to overwrite existing file</param>
        /// <returns>File name of music file for database reference</returns>
        string SaveAudioFile(string stagedFilePath, TrackCategory trackType, string genre, string artist, string album, string track, int trackNumber, int trackCount, bool overwrite = false);

        /// <summary>
        /// Stores image given album / artist / genre / file type / (storage type)
        /// </summary>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <param name="specificFileName">This would be for non-tag related images:  FanArt, ..</param>
        /// <param name="overwrite">Option to overwrite existing file</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <param name="fileType">File type related to usage</param>
        /// <param name="storageType">Storage type (temp / permanent)</param>
        /// <returns>Location of file for adding to the database file reference</returns>
        string StoreImage(BitmapImageData imageData, string genre, string artist, string album, FileTypes fileType, StorageType storageType = StorageType.DiskCache, bool overwrite = false, string specificFileName = "");

        /// <summary>
        /// Stores image given album / artist / genre / file type / (storage type)
        /// </summary>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <param name="specificFileName">This would be for non-tag related images:  FanArt, ..</param>
        /// <param name="overwrite">Option to overwrite existing file</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <param name="fileType">File type related to usage</param>
        /// <param name="storageType">Storage type (temp / permanent)</param>
        /// <returns>Location of file for adding to the database file reference</returns>
        string StoreImage(ATL.PictureInfo pictureInfo, string genre, string artist, string album, FileTypes fileType, StorageType storageType = StorageType.DiskCache, bool overwrite = false, string specificFileName = "");
    }
}
