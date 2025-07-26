using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IModelFileService))]
    public class ModelFileService : IModelFileService
    {
        private readonly IFileController _fileController;
        private readonly IModelValidationService _modelValidationService;

        [IocImportingConstructor]
        public ModelFileService(IFileController fileController, IModelValidationService modelValidationService)
        {
            _fileController = fileController;
            _modelValidationService = modelValidationService;
        }

        public string CalculateFileName(IAudioStationTag tag, LibraryEntryNamingType namingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(tag, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFileName(namingType,
                                     tag.Title,
                                     tag.AlbumArtist,
                                     tag.Album,
                                     tag.Track,
                                     tag.TrackTotal);
        }

        public string CalculateFolderPath(IAudioStationTag tag, string destinationFolderBase, LibraryEntryGroupingType groupingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(tag, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFolderPath(groupingType,
                                       destinationFolderBase,
                                       tag.AlbumArtist,
                                       tag.Album,
                                       tag.Genre);
        }

        private string CalculateFileName(LibraryEntryNamingType namingType,
                                         string trackTitle,
                                         string primaryAlbumArtist,
                                         string album,
                                         uint trackNumber,
                                         uint trackCount)
        {
            switch (namingType)
            {
                case LibraryEntryNamingType.None:
                case LibraryEntryNamingType.Standard:
                {
                    var format = "{0:#} of {1:#} {2}.mp3";
                    var formattedTitle = string.Format(format, trackNumber, trackCount, trackTitle);
                    return _fileController.MakeFriendlyPath(true, formattedTitle);
                }
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} of {1:#} {2}-{3}-{4}.mp3";
                    var formattedTitle = string.Format(format, trackNumber, trackCount, primaryAlbumArtist, album, trackTitle);
                    return _fileController.MakeFriendlyPath(true, formattedTitle);
                }
                default:
                    throw new Exception("Unhandled naming type:  ModelFileService.cs");
            }
        }

        private string CalculateFolderPath(LibraryEntryGroupingType groupingType,
                                           string destinationFolderBase,
                                           string primaryAlbumArtist,
                                           string album,
                                           string genre)
        {
            switch (groupingType)
            {
                case LibraryEntryGroupingType.None:
                    return destinationFolderBase;
                case LibraryEntryGroupingType.ArtistAlbum:
                {
                    var artistFolder = _fileController.MakeFriendlyPath(false, primaryAlbumArtist);
                    var albumFolder = _fileController.MakeFriendlyPath(false, album);

                    return Path.Combine(destinationFolderBase, artistFolder, albumFolder);
                }
                case LibraryEntryGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = _fileController.MakeFriendlyPath(false, primaryAlbumArtist);
                    var albumFolder = _fileController.MakeFriendlyPath(false, album);
                    var genreFolder = _fileController.MakeFriendlyPath(false, genre);

                    return Path.Combine(destinationFolderBase, genre, artistFolder, albumFolder);
                }
                default:
                    throw new Exception("Unhandled grouping type:  LibraryLoaderImportWorker.cs");
            }
        }
    }
}
