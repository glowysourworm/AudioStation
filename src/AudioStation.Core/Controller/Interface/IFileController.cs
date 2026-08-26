using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Model;

namespace AudioStation.Core.Controller.Interface
{
    public interface IFileController
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
        /// Returns the image for the specified album / artist / file type / (storage type)
        /// </summary>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="fileType">File type related to usage</param>
        /// <param name="storageType">Storage type (temp / permanent)</param>
        /// <returns>Image data ready for use for WPF controls</returns>
        BitmapImageData GetImage(string album, string artist, FileTypes fileType, StorageType storageType = StorageType.DiskCache);

        /// <summary>
        /// Stores image given album / artist / file type / (storage type)
        /// </summary>
        /// <param name="imageData">Image data from other services</param>
        /// <param name="album">Album related to the image (from database entities)</param>
        /// <param name="artist">Artist related to the image (from database entities)</param>
        /// <param name="fileType">File type related to usage</param>
        /// <param name="storageType">Storage type (temp / permanent)</param>
        /// <returns>Location of file for adding to the database file reference</returns>
        string StoreImage(BitmapImageData imageData, string album, string artist, FileTypes fileType, StorageType storageType = StorageType.DiskCache);
    }
}
