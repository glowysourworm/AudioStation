using System.IO;

using AudioStation.Core.Component.BitmapConverterComponent;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.ImageCacheModel;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility.FileUtility;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IFileController))]
    public class FileController : IFileController
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IBitmapConverter _bitmapConverter;

        private const string FAN_ART_DIRECTORY_NAME = "FanArt";
        private const string FRONT_COVER_FILE_NAME = "FrontCover.bmp";
        private const string BACK_COVER_FILE_NAME = "BackCover.bmp";

        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public FileController(IConfigurationManager configurationManager, IBitmapConverter bitmapConverter)
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
            BitmapImageData imageData,
            string album,
            string artist,
            string genre,
            FileTypes fileType,
            string specificFileName = "",
            IFileController.StorageType storageType = IFileController.StorageType.DiskCache,
            bool overwrite = false)
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
                // Calculate Path:  Also, create intermediate directories
                var finalPath = CalculateFilePath(genre, artist, album, specificFileName, fileType, TrackType.Any, storageType);

                // -> Save
                StoreImageFileImpl(imageData, finalPath, overwrite);

                return finalPath;
            }
            catch (Exception ex)
            {
                throw new Exception("Error storing image file", ex);
            }
        }

        public string SaveAudioFile(string stagedFilePath, TrackType trackType, string genre, string album, string artist, string track, int trackNumber, int trackCount, bool overwrite = false)
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

                // Calculate Track File Name:  needs all info from a valid tag to proceed
                var fileName = CalculateTrackFileName(configuration.ImportNamingType, track, artist, album, trackNumber, trackCount);

                // Calculate Path:  Also, create intermediate directories
                var finalPath = CalculateFilePath(genre, artist, album, fileName, FileTypes.AudioFile, trackType, IFileController.StorageType.DiskPermanent);

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

        private string CalculateFilePath(string genre,
                                         string artist,
                                         string album,
                                         string fileName,
                                         FileTypes fileType,
                                         TrackType trackType,
                                         IFileController.StorageType storageType)
        {
            var configuration = _configurationManager.GetValidConfiguration();

            string baseFolder = string.Empty;

            // Audio
            if (fileType == FileTypes.AudioFile)
            {
                switch (trackType)
                {
                    case TrackType.Music:
                        baseFolder = Path.Combine(configuration.DirectoryBase, configuration.MusicSubDirectory);
                        break;
                    case TrackType.AudioBook:
                        baseFolder = Path.Combine(configuration.DirectoryBase, configuration.AudioBooksSubDirectory);
                        break;
                    case TrackType.Any:
                    default:
                        throw new Exception("Unhandled track type");
                }
            }


            // Images
            else
            {
                // Select cache folder or permanent folder
                baseFolder = storageType == IFileController.StorageType.DiskCache ?
                                                configuration.ApplicationCacheFolder :
                                                configuration.ApplicationStorageFolder;
            }

            string folderPath = string.Empty;

            try
            {
                // Audio Folder Path:  This is where music files are typically placed. The same folder path will
                //                     be used for cache / permanent storage to organize artwork or other files.
                folderPath = CalculateAudioFolderPath(configuration.ImportGroupingType, baseFolder, artist, album, genre, true);
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
                                              string trackTitle,
                                              string artist,
                                              string album,
                                              int trackNumber,
                                              int trackCount)
        {
            switch (namingType)
            {
                case TrackNamingType.None:
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

        private string CalculateAudioFolderPath(TrackGroupingType groupingType,
                                                string destinationFolderBase,
                                                string artist,
                                                string album,
                                                string genre,
                                                bool createFolders = false)
        {
            if (!Directory.Exists(destinationFolderBase))
                throw new ArgumentException("Directory does not exist:  " + destinationFolderBase);

            switch (groupingType)
            {
                case TrackGroupingType.None:
                    return destinationFolderBase;
                case TrackGroupingType.ArtistAlbum:
                {
                    var artistFolder = MigrationHelpers.MakeFriendlyPath(false, artist);
                    var albumFolder = MigrationHelpers.MakeFriendlyPath(false, album);

                    // ../Artist
                    if (createFolders && !Directory.Exists(artistFolder))
                        Directory.CreateDirectory(artistFolder);

                    // ../Artist/Album
                    if (createFolders && !Directory.Exists(albumFolder))
                        Directory.CreateDirectory(albumFolder);

                    return Path.Combine(destinationFolderBase, artistFolder, albumFolder);
                }
                case TrackGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = MigrationHelpers.MakeFriendlyPath(false, artist);
                    var albumFolder = MigrationHelpers.MakeFriendlyPath(false, album);
                    var genreFolder = MigrationHelpers.MakeFriendlyPath(false, genre);

                    // ../Genre
                    if (createFolders && !Directory.Exists(genreFolder))
                        Directory.CreateDirectory(genreFolder);

                    // ../Genre/Artist
                    if (createFolders && !Directory.Exists(artistFolder))
                        Directory.CreateDirectory(artistFolder);

                    // ../Genre/Artist/Album
                    if (createFolders && !Directory.Exists(albumFolder))
                        Directory.CreateDirectory(albumFolder);

                    return Path.Combine(destinationFolderBase, genre, artistFolder, albumFolder);
                }
                default:
                    throw new Exception("Unhandled grouping type:  LibraryLoaderImportWorker.cs");
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
        public Task<IAudioStationService.Status> Initialize(AudioStationConfiguration configuration)
        {
            // Create Configuration Folders

            // Cache
            if (!string.IsNullOrWhiteSpace(configuration.ApplicationCacheFolder))
            {
                if (!Directory.Exists(configuration.ApplicationCacheFolder))
                    Directory.CreateDirectory(configuration.ApplicationCacheFolder);
            }

            // Storage
            if (!string.IsNullOrWhiteSpace(configuration.ApplicationStorageFolder))
            {
                if (!Directory.Exists(configuration.ApplicationStorageFolder))
                    Directory.CreateDirectory(configuration.ApplicationStorageFolder);
            }

            // Library -> Music
            if (!string.IsNullOrWhiteSpace(configuration.MusicSubDirectory))
            {
                var musicDirectory = Path.Combine(configuration.DirectoryBase, configuration.MusicSubDirectory);

                if (!Directory.Exists(musicDirectory))
                    Directory.CreateDirectory(musicDirectory);
            }

            // Library -> Audio Books
            if (!string.IsNullOrWhiteSpace(configuration.AudioBooksSubDirectory))
            {
                var audioBooksDirectory = Path.Combine(configuration.DirectoryBase, configuration.AudioBooksSubDirectory);

                if (!Directory.Exists(audioBooksDirectory))
                    Directory.CreateDirectory(audioBooksDirectory);
            }

            return Task.FromResult(IAudioStationService.Status.Idle);
        }
        public Task<IAudioStationService.Status> ReInitialize(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationService.Status.Idle);
        }
        public string GetStatusMessage()
        {
            return IAudioStationService.GetDefaultStatusMessage(GetStatus());
        }
        #endregion
    }
}
