using System.IO;

using ATL;

using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.ImageCacheModel;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility.FileUtility;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IFileController))]
    public class FileController : IFileController
    {
        private readonly IAudioStationConfigurationManager _configurationManager;
        private readonly IBitmapConverter _bitmapConverter;

        private const string FAN_ART_DIRECTORY_NAME = "FanArt";
        private const string FRONT_COVER_FILE_NAME = "FrontCover.bmp";
        private const string BACK_COVER_FILE_NAME = "BackCover.bmp";

        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public FileController(IAudioStationConfigurationManager configurationManager, IBitmapConverter bitmapConverter)
        {
            _configurationManager = configurationManager;
            _bitmapConverter = bitmapConverter;
        }

        public BitmapImageData GetImage(string fullPath)
        {
            try
            {
                var extension = Path.GetExtension(fullPath);
                var mimeType = ImageMimeTypes.Bitmap;

                switch (extension)
                {
                    case ".bmp":
                        mimeType = ImageMimeTypes.Bitmap;
                        break;
                    case ".jpeg":
                    case ".jpg":
                        mimeType = ImageMimeTypes.Jpeg;
                        break;
                    case ".png":
                        mimeType = ImageMimeTypes.Png;
                        break;
                    default:
                        throw new Exception("Unhandled image file extension");
                }

                var buffer = File.ReadAllBytes(fullPath);

                return _bitmapConverter.BitmapDataToBitmapSource(buffer, new ImageSize(ImageCacheType.FullSize), mimeType);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieveing image", ex);
            }
        }

        public string StoreImage(
            PictureInfo pictureInfo,
            string genre,
            string artist,
            string album,
            FileTypes fileType,
            IFileController.StorageType storageType = IFileController.StorageType.DiskCache,
            bool overwrite = false,
            string specificFileName = "")
        {
            try
            {
                // Convert -> BMP
                var bitmapData = _bitmapConverter.BitmapDataToBitmapSource(pictureInfo.PictureData, new ImageSize(ImageCacheType.FullSize), pictureInfo.MimeType);

                return StoreImage(bitmapData, genre, artist, album, fileType, storageType, overwrite, specificFileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error storing image", ex);
            }
        }

        public string StoreImage(
            BitmapImageData imageData,
            string genre,
            string artist,
            string album,
            FileTypes fileType,
            IFileController.StorageType storageType = IFileController.StorageType.DiskCache,
            bool overwrite = false,
            string specificFileName = "")
        {
            // Procedure:
            //
            // 0) Get file storage preferences from configuration
            // 1) Calculate file name for the image
            // 2) Save file to disk
            // 3) Return the file name
            //

            try
            {
                var configuration = _configurationManager.GetValidConfiguration();

                var libraryDirectory = (storageType == IFileController.StorageType.DiskCache) ?
                                            configuration.ApplicationCacheFolder :
                                            configuration.ApplicationStorageFolder;

                // Calculate Path:  Also, create intermediate directories
                var finalPath = CalculateFilePath(libraryDirectory, genre, artist, album, specificFileName, fileType, TrackType.Any, storageType);

                // -> Save
                StoreImageFileImpl(imageData, finalPath, overwrite);

                return finalPath;
            }
            catch (Exception ex)
            {
                throw new Exception("Error storing image file", ex);
            }
        }

        public string SaveAudioFile(string stagedFilePath, TrackType trackType, string genre, string artist, string album, string track, int trackNumber, int trackCount, bool overwrite = false)
        {
            // Procedure:
            //
            // 0) Get file storage preferences from configuration
            // 1) Calculate file name for the image
            // 2) Save file to disk
            // 3) Return the file name
            //

            try
            {
                var configuration = _configurationManager.GetValidConfiguration();
                var libraryDirectory = GetLibraryDirectory(stagedFilePath);

                // Calculate Track File Name:  needs all info from a valid tag to proceed
                var fileName = CalculateTrackFileName(libraryDirectory.NamingType, stagedFilePath, track, artist, album, trackNumber, trackCount);

                // Calculate Path:  Also, create intermediate directories
                var finalPath = CalculateFilePath(libraryDirectory, genre, artist, album, fileName, FileTypes.AudioFile, trackType, IFileController.StorageType.DiskPermanent);

                if (File.Exists(finalPath))
                {
                    if (overwrite)
                        File.Delete(finalPath);

                    else
                        throw new Exception("File already exists:  " + finalPath);
                }

                // Read (Staged)
                var buffer = File.ReadAllBytes(stagedFilePath);

                // Write (Library)
                File.WriteAllBytes(finalPath, buffer);

                return finalPath;
            }
            catch (Exception ex)
            {
                throw new Exception("Error storing image file", ex);
            }
        }

        // Implementation: Exceptions thrown for normal file system issues
        private void StoreImageFileImpl(BitmapImageData imageData, string finalPath, bool overwrite)
        {
            // Overwrite (?)
            if (File.Exists(finalPath))
            {
                if (overwrite)
                    File.Delete(finalPath);

                else
                    throw new Exception("File already exists:  " + finalPath);
            }

            // Write
            File.WriteAllBytes(finalPath, imageData.GetBuffer());
        }

        private string CalculateFilePath(ILibraryDirectory libraryDirectory,
                                         string genre,
                                         string artist,
                                         string album,
                                         string fileName,
                                         FileTypes fileType,
                                         TrackType trackType,
                                         IFileController.StorageType storageType)
        {
            // Path off of the library directory
            string folderPath = string.Empty;

            try
            {
                // Audio Folder Path:  This is where music files are typically placed. The same folder path will
                //                     be used for cache / permanent storage to organize artwork or other files.
                folderPath = CalculateAudioFolderPath(libraryDirectory, genre, artist, album, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating storage:  " + ex.Message);
            }


            switch (fileType)
            {
                case FileTypes.FrontCover:
                {
                    return Path.Combine(folderPath, FRONT_COVER_FILE_NAME);
                }
                case FileTypes.BackCover:
                {
                    return Path.Combine(folderPath, BACK_COVER_FILE_NAME);
                }
                case FileTypes.FanArt:
                {
                    var fanArtDirectory = Path.Combine(folderPath, FAN_ART_DIRECTORY_NAME);

                    if (!Directory.Exists(fanArtDirectory))
                        Directory.CreateDirectory(fanArtDirectory);

                    return Path.Combine(fanArtDirectory, fileName);
                }
                case FileTypes.AudioFile:
                {
                    return Path.Combine(folderPath, fileName);
                }
                default:
                    throw new Exception("Unhandled file type:  CalculateImageFileName");
            }
        }

        private string CalculateTrackFileName(TrackNamingType namingType,
                                              string originalFileName,
                                              string trackTitle,
                                              string artist,
                                              string album,
                                              int trackNumber,
                                              int trackCount)
        {
            switch (namingType)
            {
                case TrackNamingType.None:
                    return MigrationHelpers.MakeFriendlyPath(true, originalFileName);
                case TrackNamingType.Standard:
                {
                    var format = "{0:#} of {1:#} {2}.mp3";
                    var formattedTitle = string.Format(format, trackNumber, trackCount, trackTitle);
                    return MigrationHelpers.MakeFriendlyPath(true, formattedTitle);
                }
                case TrackNamingType.Descriptive:
                {
                    var format = "{0:#} of {1:#} {2}-{3}-{4}.mp3";
                    var formattedTitle = string.Format(format, trackNumber, trackCount, artist, album, trackTitle);
                    return MigrationHelpers.MakeFriendlyPath(true, formattedTitle);
                }
                default:
                    throw new Exception("Unhandled naming type:  ModelFileService.cs");
            }
        }

        private string CalculateAudioFolderPath(ILibraryDirectory libraryDirectory,
                                                string genre,
                                                string artist,
                                                string album,
                                                bool createFolders = false)
        {
            switch (libraryDirectory.GroupingType)
            {
                case TrackGroupingType.None:
                    return libraryDirectory.Directory;

                case TrackGroupingType.ArtistAlbum:
                {
                    var artistFolder = MigrationHelpers.MakeFriendlyPath(false, artist);
                    var albumFolder = MigrationHelpers.MakeFriendlyPath(false, album);

                    var artistPath = Path.Combine(libraryDirectory.Directory, artistFolder);
                    var albumPath = Path.Combine(libraryDirectory.Directory, artistFolder, albumFolder);

                    // ../Artist
                    if (createFolders && !Directory.Exists(artistPath))
                        Directory.CreateDirectory(artistPath);

                    // ../Artist/Album
                    if (createFolders && !Directory.Exists(albumPath))
                        Directory.CreateDirectory(albumPath);

                    return albumPath;
                }
                case TrackGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = MigrationHelpers.MakeFriendlyPath(false, artist);
                    var albumFolder = MigrationHelpers.MakeFriendlyPath(false, album);
                    var genreFolder = MigrationHelpers.MakeFriendlyPath(false, genre);

                    var artistPath = Path.Combine(libraryDirectory.Directory, artistFolder);
                    var albumPath = Path.Combine(libraryDirectory.Directory, artistFolder, albumFolder);
                    var genrePath = Path.Combine(libraryDirectory.Directory, genreFolder, artistFolder, albumFolder);

                    // ../Genre
                    if (createFolders && !Directory.Exists(genrePath))
                        Directory.CreateDirectory(genrePath);

                    // ../Genre/Artist
                    if (createFolders && !Directory.Exists(artistPath))
                        Directory.CreateDirectory(artistPath);

                    // ../Genre/Artist/Album
                    if (createFolders && !Directory.Exists(albumPath))
                        Directory.CreateDirectory(albumPath);

                    return genrePath;
                }
                default:
                    throw new Exception("Unhandled grouping type:  LibraryLoaderImportWorker.cs");
            }
        }

        private ILibraryDirectory GetLibraryDirectory(string fileName)
        {
            try
            {
                var configuration = _configurationManager.GetValidConfiguration();

                var directory = new DirectoryInfo(fileName);

                if (directory.FullName == configuration.ApplicationCacheFolder.Directory)
                    return configuration.ApplicationCacheFolder;

                else if (directory.FullName == configuration.ApplicationStorageFolder.Directory)
                    return configuration.ApplicationStorageFolder;

                else if (directory.FullName == configuration.StagingFolder.Directory)
                    return configuration.StagingFolder;

                else if (directory.FullName == configuration.DownloadFolder.Directory)
                    return configuration.DownloadFolder;

                var libraryDirectory = configuration.LibraryDirectories
                                                    .FirstOrDefault(x => x.Directory == directory.FullName);

                if (libraryDirectory == null)
                    throw new Exception("Directory does not exist");

                return libraryDirectory;
            }
            catch (Exception ex)
            {
                throw new Exception("Library directory error for:  " + fileName, ex);
            }
        }

        #region IAudioStationService
        public string GetName()
        {
            return "File Controller";
        }
        public string GetDisplayName()
        {
            return "File Controller";
        }
        public IAudioStationService.Status GetStatus()
        {
            return IAudioStationService.Status.Idle;
        }
        public Task<IAudioStationService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() =>
            {
                return Initialize(configuration);
            });
        }
        public IAudioStationService.Status Initialize(AudioStationConfiguration configuration)
        {
            // Create Configuration Folders

            // Cache
            if (!string.IsNullOrWhiteSpace(configuration.ApplicationCacheFolder.Directory))
            {
                if (!Directory.Exists(configuration.ApplicationCacheFolder.Directory))
                    Directory.CreateDirectory(configuration.ApplicationCacheFolder.Directory);
            }

            // Storage
            if (!string.IsNullOrWhiteSpace(configuration.ApplicationStorageFolder.Directory))
            {
                if (!Directory.Exists(configuration.ApplicationStorageFolder.Directory))
                    Directory.CreateDirectory(configuration.ApplicationStorageFolder.Directory);
            }

            // Staging
            if (!string.IsNullOrWhiteSpace(configuration.StagingFolder.Directory))
            {
                if (!Directory.Exists(configuration.StagingFolder.Directory))
                    Directory.CreateDirectory(configuration.StagingFolder.Directory);
            }

            // Download
            if (!string.IsNullOrWhiteSpace(configuration.DownloadFolder.Directory))
            {
                if (!Directory.Exists(configuration.DownloadFolder.Directory))
                    Directory.CreateDirectory(configuration.DownloadFolder.Directory);
            }

            // User Folders
            foreach (var directory in configuration.LibraryDirectories)
            {
                if (!string.IsNullOrWhiteSpace(directory.Directory))
                {
                    if (!Directory.Exists(directory.Directory))
                        Directory.CreateDirectory(directory.Directory);
                }
            }

            return IAudioStationService.Status.Idle;
        }
        public Task<IAudioStationService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(ReInitialize(configuration));
        }
        public IAudioStationService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }
        public string GetStatusMessage()
        {
            return IAudioStationService.GetDefaultStatusMessage(GetStatus());
        }
        #endregion
    }
}
