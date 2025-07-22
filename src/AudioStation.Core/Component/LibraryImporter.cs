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
using AudioStation.Model;
using Microsoft.Extensions.Logging;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput.Interface;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ILibraryImporter))]
    public class LibraryImporter : ILibraryImporter
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IModelValidationService _modelValidationService;
        private readonly IModelFileService _modelFileService;
        private readonly IModelController _modelController;
        private readonly ITagCacheController _tagCacheController;
        private readonly IFileController _fileController;

        private const int ACOUSTID_MIN_SCORE = 0;

        [IocImportingConstructor]
        public LibraryImporter(IAcoustIDClient acoustIDClient, 
                               IMusicBrainzClient musicBrainzClient,
                               IModelValidationService modelValidationService,
                               IModelFileService modelFileService,
                               IModelController modelController,
                               ITagCacheController tagCacheController,
                               IFileController fileController) 
        {
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient  = musicBrainzClient;
            _modelValidationService = modelValidationService;
            _modelFileService = modelFileService;
            _modelController = modelController;
            _tagCacheController = tagCacheController;
            _fileController = fileController;
        }

        public bool CanImportAcoustID(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            return workInput.IdentifyUsingAcoustID;
        }
        public bool CanImportMusicBrainzBasic(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            return CanImportAcoustID(workInput, workOutput) && workInput.IncludeMusicBrainzDetail;
        }
        public bool CanImportMusicBrainzDetail(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            return CanImportMusicBrainzDetail(workInput, workOutput);
        }
        public bool CanImportEmbedTag(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            return  !string.IsNullOrEmpty(workInput.SourceFile) &&
                    _tagCacheController.Verify(workInput.SourceFile) &&
                    workOutput.FinalQueryRecord != null;
        }
        public bool CanImportEntity(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            var message = string.Empty;

            return _modelValidationService.ValidateTagImport(workOutput.ImportedTagFile, out message);
                   
        }
        public bool CanImportMigrateFile(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            return workOutput.Mp3FileImportSuccess && _fileController.CanMigrateFile(workInput.SourceFile, workOutput.DestinationPathCalculated);
        }


        public async Task<bool> WorkAcoustID(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            var acoustIDResults = await _acoustIDClient.IdentifyFingerprint(workInput.SourceFile, ACOUSTID_MIN_SCORE);

            // Output -> AcoustID Results
            workOutput.AcoustIDResults = acoustIDResults;
            workOutput.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();

            return workOutput.AcoustIDSuccess;
        }
        public async Task<bool> WorkMusicBrainzDetail(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            var acoustIDResults = workOutput.AcoustIDResults
                                            .OrderByDescending(x => x.Score)
                                            .SelectMany(x => x.Recordings)
                                            .ToList();

            var matches = new List<MusicBrainzRecording>();

            foreach (var result in acoustIDResults)
            {
                // -> Music Brainz Recording Lookup
                //
                var recording = await _musicBrainzClient.GetRecordingById(new Guid(result.Id));

                // Validation
                if (_modelValidationService.ValidateMusicBrainzRecordingImport(recording))
                {
                    // Results
                    matches.Add(recording);
                }
            }

            // Output -> Music Brainz Recording Matches (heavy load)
            workOutput.MusicBrainzRecordingMatches = matches;

            // Validate Results
            workOutput.MusicBrainzRecordingMatchSuccess = workOutput.MusicBrainzRecordingMatches.Any();

            return workOutput.MusicBrainzRecordingMatchSuccess;
        }
        public async Task<bool> WorkMusicBrainzCompleteRecord(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            await GetMusicBrainzCompletedRecord(workInput, workOutput);

            // Music Brainz Detail Result
            return workOutput.MusicBrainzCombinedRecordQuerySuccess;
        }
        public bool WorkEmbedTag(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            // Embed tag data into the SOURCE FILE
            return EmbedTagData(workInput, workOutput);
        }
        public bool WorkImportEntity(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
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
        }
        public bool WorkMigrateFile(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            // Move File, and update the entity (database)
            workOutput.Mp3FileMoveSuccess = MigrateFile(workInput, workOutput);

            return workOutput.Mp3FileMoveSuccess;
        }

        private Task GetMusicBrainzCompletedRecord(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
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
        private void CalculateFileName(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            // Calculate standard file name for the import
            var calculatedFileName = _modelFileService.CalculateFileName(workOutput.ImportedTagFile, workInput.NamingType);

            // All groupings are based on the destination file folder (base)
            var destinationFolder = _modelFileService.CalculateFolderPath(workOutput.ImportedTagFile, 
                                                                          workOutput.DestinationFolderBase, 
                                                                          workInput.GroupingType);

            // Final Impot Path
            workOutput.DestinationPathCalculated = Path.Combine(destinationFolder, calculatedFileName);
        }
        private void LoadTagData(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
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
        private bool EmbedTagData(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
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
        private bool MigrateFile(ILibraryLoaderImportLoad workInput, ILibraryLoaderImportOutput workOutput)
        {
            try
            {
                // Evict the cache (also, prior to moving in case the move fails)
                _tagCacheController.Evict(workInput.SourceFile);

                // Migrate File
                _fileController.MigrateFile(workInput.SourceFile,
                                            workOutput.DestinationPathCalculated,
                                            workInput.MigrationOverwriteDestinationFiles,
                                            workInput.MigrationDeleteSourceFiles,
                                            workInput.MigrationDeleteSourceFolders);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error migrating file:  {0} to {1}", LogMessageType.General, LogLevel.Error, ex, workInput.SourceFile, workOutput.DestinationPathCalculated);
                return false;
            }
        }
    }
}
