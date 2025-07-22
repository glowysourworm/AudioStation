using System.Formats.Tar;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

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

        public string CalculateFileName(ISimpleTag simpleTag, LibraryEntryNamingType namingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(simpleTag, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFileName(namingType,
                                     simpleTag.Title,
                                     simpleTag.FirstAlbumArtist,
                                     simpleTag.Album,
                                     simpleTag.Track,
                                     simpleTag.TrackCount);
        }

        public string CalculateFileName(TagLib.File tagFile, LibraryEntryNamingType namingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(tagFile, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFileName(namingType,
                                     tagFile.Tag.Title,
                                     tagFile.Tag.FirstAlbumArtist,
                                     tagFile.Tag.Album,
                                     tagFile.Tag.Track,
                                     tagFile.Tag.TrackCount);
        }

        public string CalculateFolderPath(ISimpleTag simpleTag, string destinationFolderBase, LibraryEntryGroupingType groupingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(simpleTag, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFolderPath(groupingType,
                                       destinationFolderBase,
                                       simpleTag.FirstAlbumArtist,
                                       simpleTag.Album,
                                       simpleTag.FirstGenre);
        }

        public string CalculateFolderPath(TagLib.File tagFile, string destinationFolderBase, LibraryEntryGroupingType groupingType)
        {
            string message;

            if (!_modelValidationService.ValidateTagImport(tagFile, out message))
                throw new ArgumentException("Invalid Tag File:  Not ready for migration. Must complete the tag minimum requirements: " + message);

            return CalculateFolderPath(groupingType,
                                       destinationFolderBase,
                                       tagFile.Tag.FirstAlbumArtist,
                                       tagFile.Tag.Album,
                                       tagFile.Tag.FirstGenre);
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
                    var formattedTitle = string.Format(format, trackNumber, trackTitle);
                    return _fileController.MakeFriendlyPath(true, formattedTitle);
                }
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} {1}-{2}-{3}.mp3";
                    var formattedTitle = string.Format(format, trackNumber, primaryAlbumArtist, album, trackTitle);
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
