using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AudioStation.Controller;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.Extensions.Collection;

using TagLib;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ILibraryImporter))]
    public class LibraryImporter : ILibraryImporter
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IModelValidationService _modelValidationService;
        private readonly IModelController _modelController;
        private readonly ITagCacheController _tagCacheController;
        private readonly IFileController _fileController;

        private const int ACOUSTID_MIN_SCORE = 0;

        [IocImportingConstructor]
        public LibraryImporter(IAcoustIDClient acoustIDClient, 
                               IMusicBrainzClient musicBrainzClient,
                               IModelValidationService modelValidationService,
                               IModelController modelController,
                               ITagCacheController tagCacheController,
                               IFileController fileController) 
        {
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient  = musicBrainzClient;
            _modelValidationService = modelValidationService;
            _modelController = modelController;
            _tagCacheController = tagCacheController;
            _fileController = fileController;
        }

        public bool CanImportAcoustID(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return workInput.IdentifyUsingAcoustID;
        }
        public bool CanImportMusicBrainzBasic(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return CanImportAcoustID(workInput, workOutput) && workInput.IncludeMusicBrainzDetail;
        }
        public bool CanImportMusicBrainzDetail(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return CanImportMusicBrainzDetail(workInput, workOutput);
        }
        public bool CanImportEmbedTag(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return  !string.IsNullOrEmpty(workInput.SourceFile) &&
                    _tagCacheController.Verify(workInput.SourceFile) &&
                    workOutput.FinalQueryRecord != null;
        }
        public bool CanImportEntity(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return _modelValidationService.ValidateTagImport(workOutput.ImportedTagFile);
                   
        }
        public bool CanImportMigrateFile(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return workOutput.Mp3FileImportSuccess && _fileController.CanMigrateFile(workInput.SourceFile, workOutput.DestinationPathCalculated);
        }


        private async Task<bool> WorkAcoustID(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            var acoustIDResults = await _acoustIDClient.IdentifyFingerprint(workInput.SourceFile, ACOUSTID_MIN_SCORE);

            // Output -> AcoustID Results
            workOutput.AcoustIDResults = acoustIDResults;
            workOutput.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();

            return workOutput.AcoustIDSuccess;
        }
        private Task<bool> WorkMusicBrainzDetail(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return Task.Run<bool>(() =>
            {
                // Output -> Music Brainz Recording Matches (heavy load)
                workOutput.MusicBrainzRecordingMatches = workOutput.AcoustIDResults
                                                                   .OrderByDescending(x => x.Score)
                                                                   .SelectMany(x => x.Recordings)
                                                                   .Select(x => _musicBrainzClient.GetRecordingById(new Guid(x.Id)).Result)
                                                                   .Where(x => _modelValidationService.ValidateMusicBrainzRecordingImport(x))
                                                                   .ToList();
                // Validate Results
                workOutput.MusicBrainzRecordingMatchSuccess = workOutput.MusicBrainzRecordingMatches.Any();

                return workOutput.MusicBrainzRecordingMatchSuccess;
            });
        }
        private async Task<bool> WorkMusicBrainzCompleteRecord(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            await GetMusicBrainzCompletedRecord(workInput, workOutput);

            // Music Brainz Detail Result
            return workOutput.MusicBrainzCombinedRecordQuerySuccess;
        }
        private bool WorkEmbedTag(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            // Embed tag data into the SOURCE FILE
            return EmbedTagData(workInput, workOutput);
        }
        private Task<bool> WorkImportEntity(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return Task.Run(() =>
            {
                // Calculate destination file name. Destination path is used no matter if there is a file migration or not.
                CalculateFileName(workInput, workOutput);

                // Import Record:  Save imported entity for output
                workOutput.ImportedRecord = _modelController.AddUpdateLibraryEntry(workOutput.DestinationPathCalculated,
                                                                                    workOutput.ImportedTagFileAvailable,
                                                                                    workOutput.ImportedTagFileLoadError,
                                                                                    workOutput.ImportedTagFileErrorMessage,
                                                                                    workOutput.ImportedTagFile);

                workOutput.Mp3FileImportSuccess = workOutput.ImportedRecord != null;

                return workOutput.Mp3FileImportSuccess;
            });
        }
        private Task<bool> WorkMigrateFile(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return Task.Run(() =>
            {
                // Move File, and update the entity (database)
                workOutput.Mp3FileMoveSuccess = MoveFile(workInput, workOutput);

                return workOutput.Mp3FileMoveSuccess;
            });
        }

        private Task GetMusicBrainzCompletedRecord(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            return Task.Run(() =>
            {

                // Take the first result - the rest may be presented to the user for further processing
                var bestMatch = workOutput.MusicBrainzRecordingMatches
                                          .FirstOrDefault(x => _modelValidationService.ValidateMusicBrainzRecordingImport(x));

                if (bestMatch != null)
                {
                    // Success (Call / Cache MusicBrainz) (heavy load)
                    var recordMatches = _modelController.GetCompleteMusicBrainzRecord(bestMatch.ArtistCredit.First().Name,
                                                                                      bestMatch.Releases.First().Title,
                                                                                      bestMatch.Title);

                    // Output -> Music Brainz Combined Query Records
                    workOutput.MusicBrainzCombinedRecordQuerySuccess = recordMatches != null && recordMatches.Any();
                    workOutput.MusicBrainzCombinedLibraryEntryRecords = recordMatches ?? Enumerable.Empty<MusicBrainzCombinedLibraryEntryRecord>();

                    if (workOutput.MusicBrainzCombinedLibraryEntryRecords.Any())
                    {
                        // Lookup record with matching ID (try and select greedily for the artwork)
                        //
                        var record = workOutput.MusicBrainzCombinedLibraryEntryRecords.FirstOrDefault(x => x.Track.Title == bestMatch.Title);

                        if (record != null)
                        {
                            var artwork = workOutput.MusicBrainzCombinedLibraryEntryRecords.SelectMany(x => x.ReleasePictures).ToList();

                            // Scrape any artwork that is already downloaded from the results
                            var front = record.ReleasePictures.Any(x => x.Type == PictureType.FrontCover) ?
                                        record.ReleasePictures.First(x => x.Type == PictureType.FrontCover) :
                                        artwork.FirstOrDefault(x => x.Type == PictureType.FrontCover);

                            var back = record.ReleasePictures.Any(x => x.Type == PictureType.BackCover) ?
                                       record.ReleasePictures.First(x => x.Type == PictureType.BackCover) :
                                       artwork.FirstOrDefault(x => x.Type == PictureType.BackCover);

                            // Store artwork
                            workOutput.BestBackCover = back;
                            workOutput.BestFrontCover = front;

                            // Music Brainz Combined Record Query Complete
                            workOutput.MusicBrainzCombinedRecordQuerySuccess = true;
                            workOutput.FinalQueryRecord = record;
                        }
                    }
                }
            });
        }
        private void CalculateFileName(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            // All groupings are based on the destination file folder (base)
            var fileName = Path.GetFileName(workInput.SourceFile);
            var calculatedFileName = fileName;

            switch (workInput.NamingType)
            {
                case LibraryEntryNamingType.None:
                    // Calculated file name is the original file name
                    break;
                case LibraryEntryNamingType.Standard:
                {
                    var format = "{0:#} {1}-{2}.mp3";
                    var formattedTitle = string.Format(format, workOutput.ImportedTagFile.Tag.Track, workOutput.ImportedTagFile.Tag.FirstAlbumArtist, workOutput.ImportedTagFile.Tag.Track);
                    calculatedFileName = _fileController.MakeFriendlyPath(true, formattedTitle);
                }
                break;
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} {1}-{2}-{3}.mp3";
                    var formattedTitle = string.Format(format, workOutput.ImportedTagFile.Tag.Track, workOutput.ImportedTagFile.Tag.FirstGenre, workOutput.ImportedTagFile.Tag.FirstAlbumArtist, workOutput.ImportedTagFile.Tag.Track);
                    calculatedFileName = _fileController.MakeFriendlyPath(true, formattedTitle);
                }
                break;
                default:
                    throw new Exception("Unhandled naming type:  LibraryLoaderImportWorker.cs");
            }

            switch (workInput.GroupingType)
            {
                case LibraryEntryGroupingType.None:
                    workOutput.DestinationPathCalculated = Path.Combine(workOutput.DestinationFolderBase, calculatedFileName);
                    break;
                case LibraryEntryGroupingType.ArtistAlbum:
                {
                    var artistFolder = _fileController.MakeFriendlyPath(false, workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = _fileController.MakeFriendlyPath(false, workOutput.ImportedTagFile.Tag.Album);

                    workOutput.DestinationSubFolders = new string[] { artistFolder, Path.Combine(artistFolder, albumFolder) };
                    workOutput.DestinationPathCalculated = Path.Combine(workOutput.DestinationFolderBase,
                                                                        artistFolder,
                                                                        albumFolder,
                                                                        calculatedFileName);
                }
                break;
                case LibraryEntryGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = _fileController.MakeFriendlyPath(false, workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = _fileController.MakeFriendlyPath(false, workOutput.ImportedTagFile.Tag.Album);
                    var genreFolder = _fileController.MakeFriendlyPath(false, workOutput.ImportedTagFile.Tag.FirstGenre);

                    workOutput.DestinationSubFolders = new string[] { genreFolder,
                                                                      Path.Combine(genreFolder, artistFolder),
                                                                      Path.Combine(genreFolder, artistFolder, albumFolder)};

                    workOutput.DestinationPathCalculated = Path.Combine(workOutput.DestinationFolderBase,
                                                                        genreFolder,
                                                                        artistFolder,
                                                                        albumFolder,
                                                                        calculatedFileName);
                }
                break;
                default:
                    throw new Exception("Unhandled grouping type:  LibraryLoaderImportWorker.cs");
            }
        }
        private void LoadTagData(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            if (string.IsNullOrEmpty(workInput.SourceFile))
                throw new ArgumentException("Invalid media file name");

            // File load parameters
            workOutput.ImportedTagFileAvailable = Path.Exists(workInput.SourceFile);
            workOutput.ImportedTagFileLoadError = false;
            workOutput.ImportedTagFileErrorMessage = string.Empty;

            TagLib.File fileRef = null;

            try
            {
                fileRef = _tagCacheController.Get(workInput.SourceFile);

                if (fileRef == null)
                {
                    workOutput.ImportedTagFileErrorMessage = "Unable to load tag from file";
                    workOutput.ImportedTagFileLoadError = true;
                }

                workOutput.ImportedTagFile = fileRef;
            }
            catch (Exception ex)
            {
                workOutput.ImportedTagFileErrorMessage = ex.Message;
                workOutput.ImportedTagFileLoadError = true;
            }
        }
        private bool EmbedTagData(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            if (string.IsNullOrEmpty(workInput.SourceFile))
                throw new ArgumentException("Invalid media file name");

            if (workOutput.FinalQueryRecord == null)
                throw new ArgumentException("Final query record not yet completed. Embedding of tag data halted.");

            try
            {
                workOutput.ImportedTagFileAvailable = System.IO.File.Exists(workInput.SourceFile);
                workOutput.ImportedTagFileLoadError = false;
                workOutput.ImportedTagFileErrorMessage = string.Empty;

                var fileRef = _tagCacheController.Get(workInput.SourceFile);

                if (fileRef == null)
                    return false;

                fileRef.Tag.Album = workOutput.FinalQueryRecord.Release.Title;
                fileRef.Tag.AlbumArtists = workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.AlbumArtistsSort = workOutput.FinalQueryRecord.Artists.Select(x => x.SortName).ToArray();
                fileRef.Tag.AmazonId = workOutput.FinalQueryRecord.Release.Asin;
                fileRef.Tag.Artists = workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.Comment = workOutput.FinalQueryRecord.Recording.Annotation;
                fileRef.Tag.Disc = (uint)(workOutput.FinalQueryRecord.Release.Media.IndexOf(x => x.Id == workOutput.FinalQueryRecord.Medium.Id) + 1);
                fileRef.Tag.DiscCount = (uint)workOutput.FinalQueryRecord.Release.Media.Count;
                fileRef.Tag.Genres = workOutput.FinalQueryRecord.RecordingGenres.Select(x => x.Name).ToArray();
                fileRef.Tag.MusicBrainzArtistId = workOutput.FinalQueryRecord.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseArtistId = workOutput.FinalQueryRecord.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseCountry = workOutput.FinalQueryRecord.Release.Country;
                fileRef.Tag.MusicBrainzReleaseId = workOutput.FinalQueryRecord.Release.Id.ToString();
                fileRef.Tag.MusicBrainzReleaseStatus = workOutput.FinalQueryRecord.Release.Status;
                fileRef.Tag.MusicBrainzTrackId = workOutput.FinalQueryRecord.Track.Id.ToString();
                fileRef.Tag.Performers = workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.PerformersSort = workOutput.FinalQueryRecord.Artists.Select(x => x.SortName).ToArray();

                var pictures = new List<IPicture>();

                if (workOutput.BestFrontCover != null)
                    pictures.Add(workOutput.BestFrontCover);

                if (workOutput.BestBackCover != null)
                    pictures.Add(workOutput.BestBackCover);

                // COVER ART!
                fileRef.Tag.Pictures = pictures.ToArray();

                fileRef.Tag.Title = workOutput.FinalQueryRecord.Track.Title;
                fileRef.Tag.Track = (uint)(workOutput.FinalQueryRecord.Track.Position ?? 0);
                fileRef.Tag.TrackCount = (uint)workOutput.FinalQueryRecord.Medium.TrackCount;
                fileRef.Tag.Year = (uint)(workOutput.FinalQueryRecord.Release?.Date?.Year ?? 0);

                fileRef.Save();

                // Set Output
                workOutput.ImportedTagFile = fileRef;

                return true;
            }
            catch (Exception ex)
            {
                workOutput.ImportedTagFileAvailable = true;
                workOutput.ImportedTagFileLoadError = true;
                workOutput.ImportedTagFileErrorMessage = ex.Message;

                return false;
            }
        }
        private bool MoveFile(LibraryLoaderImportDetailLoad workInput, LibraryLoaderImportDetailOutput workOutput)
        {
            var message = string.Empty;

            try
            {
                // Make sure that sub-folders are in place
                if (!Directory.Exists(workInput.DestinationFolder))
                {
                    message = "Destination folder does not exist! Make sure that the music / audio books sub-folders have been created before executing import";
                    return false;
                }

                foreach (var subFolder in workOutput.DestinationSubFolders)
                {
                    var directory = Path.Combine(workInput.DestinationFolder, subFolder);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                // Verify that calculated path now is available
                var fullDirectory = Path.GetDirectoryName(workOutput.DestinationPathCalculated);

                // This should be here; but we can try using System.IO to create the directory 
                // one last time (before getting the exception)
                //
                if (!Directory.Exists(fullDirectory))
                    Directory.CreateDirectory(fullDirectory);

                // Evict the cache (also, prior to moving in case the move fails)
                _tagCacheController.Evict(workInput.SourceFile);

                System.IO.File.Move(workInput.SourceFile, workOutput.DestinationPathCalculated, true);

                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}
