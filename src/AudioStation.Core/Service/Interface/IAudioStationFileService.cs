using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Model;

namespace AudioStation.Core.Service.Interface
{
    public interface IAudioStationFileService : IAudioStationDataService
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
        /// Checks library directories to make sure file path is valid
        /// </summary>
        bool CanWriteToPath(string filePath);

        /// <summary>
        /// Copies file from source path to destination path checking the properties of the
        /// library directories.
        /// </summary>
        void CopyFileTo(string source, string destination, bool overwrite);

        /// <summary>
        /// Deletes file from disk checking the propertis of library directories.
        /// </summary>
        void DeleteFile(string filePath);

        /// <summary>
        /// Deletes (EMPTY) directory from disk checking library directory properties.
        /// </summary>
        void DeleteEmptyDirectory(string directory);

        /// <summary>
        /// Calculates file name of music (or) audio books file given the destination directory
        /// </summary>
        /// <param name="sourceFilePath">Full file path to source file</param>
        /// <param name="destinationDirectory">File full path of staged file (this should be already validated for migration)</param>
        /// <param name="trackType">Type of audio track (music, audiobook, ...)</param>
        /// <param name="track">Track title for the audio track</param>
        /// <param name="trackCount">Track count for the album</param>
        /// <param name="trackNumber">Track number for the track</param>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="genre">Genre related to the image (from database entities)</param>
        /// <returns>File name of music file for database reference</returns>
        string CalculateGivenFileName(string sourceFilePath, string destinationDirectory, TrackCategory trackType, string genre, string artist, string album, string track, int trackNumber, int trackCount, bool createIntermediateDirectories);

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
