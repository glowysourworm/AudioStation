using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions.Collection;

using TagLib;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderImportBasicWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly ITagCacheController _tagCacheController;
        private readonly IModelValidationService _modelValidationService;

        private const int WORK_STEPS = 5;
        private const int ACOUSTID_MIN_SCORE = 80;
        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        private LibraryLoaderImportBasicLoad _workLoad;
        private LibraryLoaderImportBasicOutput _workOutput;

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderImportBasicWorker(LibraryLoaderWorkItem workItem,
                                              IModelController modelController,
                                              IAcoustIDClient acoustIDClient,
                                              IMusicBrainzClient musicBrainzClient,
                                              ITagCacheController tagCacheController,
                                              IModelValidationService modelValidationService)
            : base(workItem)
        {
            _workLoad = workItem.GetWorkItem() as LibraryLoaderImportBasicLoad;
            _workOutput = workItem.GetOutputItem() as LibraryLoaderImportBasicOutput;

            _modelController = modelController;
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient = musicBrainzClient;
            _tagCacheController = tagCacheController;
            _modelValidationService = modelValidationService;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }

        public override int GetCurrentWorkStep()
        {
            lock (_lock)
            {
                return _workCurrentStep;
            }
        }

        protected override bool WorkNext()
        {
            // Steps:
            //
            // 1) AcoustID
            // 2) Music Brainz
            // 3) Embed Tag File
            // 4) Import Entity
            // 5) Migrate File (optional)
            // 

            IncrementWorkStep();

            switch (_workCurrentStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    var message = string.Empty;
                    var success = WorkAcoustID(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 2:
                {
                    var message = string.Empty;
                    var success = WorkMusicBrainzDetail(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 3:
                {
                    var message = string.Empty;
                    var success = WorkEmbedTag(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 4:
                {
                    var message = string.Empty;
                    var success = WorkImportEntity(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 5:
                {
                    var message = string.Empty;
                    var success = WorkMigrateFile(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
            }
        }

        private void IncrementWorkStep()
        {
            lock (_lock)
            {
                _workCurrentStep++;
            }
        }

        private bool WorkAcoustID(out string message)
        {
            if (_workOutput.AcoustIDSuccess)
            {
                message = "AcoustID results already completed successfully";
                return true;
            }

            var acoustIDResults = _acoustIDClient.IdentifyFingerprint(_workLoad.SourceFile, ACOUSTID_MIN_SCORE).Result;

            // Output -> AcoustID Results
            _workOutput.AcoustIDResults = acoustIDResults;
            _workOutput.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();

            message = _workOutput.AcoustIDSuccess ? "AcoustID acoustic fingerprint identification successful" : "Acoust ID acoustic fingerprint identification failed:  please see results";

            return _workOutput.AcoustIDSuccess;
        }
        private bool WorkMusicBrainzDetail(out string message)
        {
            if (_workOutput.MusicBrainzRecordingMatchSuccess)
            {
                message = "Music Brainz detail results already retrieved successfully";
                return true;
            }

            GetMusicBrainzDetail();

            // Music Brainz Detail Result
            var success = _workOutput.MusicBrainzRecordingMatchSuccess;

            message = success ? "Music Brainz lookup successful" : "Music Brainz lookup failed:  please see results";

            return success;
        }
        private bool WorkEmbedTag(out string message)
        {
            // Embed tag data into the SOURCE FILE
            EmbedTagData();

            var success = !_workOutput.ImportedTagFileLoadError && _workOutput.ImportedTagFileAvailable && _workOutput.ImportedTagFile != null;

            message = success ? "Mp3 file tag data embedded successfully" : "Error embedding Mp3 tag data:  please see results";

            return success;
        }
        private bool WorkImportEntity(out string message)
        {
            if (_workOutput.Mp3FileImportSuccess)
            {
                message = "Import of mp3 file aleady completed";
                return true;
            }

            // Calculate destination file name. Destination path is used no matter if there is a file migration or not.
            CalculateFileName();

            // Import Record:  Save imported entity for output
            _workOutput.ImportedRecord = _modelController.AddUpdateLibraryEntry(_workOutput.DestinationPathCalculated,
                                                                                _workOutput.ImportedTagFileAvailable,
                                                                                _workOutput.ImportedTagFileLoadError,
                                                                                _workOutput.ImportedTagFileErrorMessage,
                                                                                _workOutput.ImportedTagFile);

            _workOutput.Mp3FileImportSuccess = _workOutput.ImportedRecord != null;

            message = _workOutput.Mp3FileImportSuccess ? "Import of mp3 data into local database successful" : "Import of mp3 data into local database failed: please see results";

            return _workOutput.Mp3FileImportSuccess;
        }
        private bool WorkMigrateFile(out string message)
        {
            // Move File, and update the entity (database)
            _workOutput.Mp3FileMoveSuccess = MoveFile(out message);

            // Update Result
            message = _workOutput.Mp3FileMoveSuccess ? "File migration successful! Import complete!" : "Final import succeeded; but the source file could not be migrated (move failed):  please see results";

            return _workOutput.Mp3FileMoveSuccess;
        }

        public void GetAcoustIDResults(ref LibraryLoaderImportDetailOutput outputItem, string fileFullName)
        {
            // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
            //          get; and hope that it works right out of the box.
            //
            var acoustIDResults = _acoustIDClient.IdentifyFingerprint(fileFullName, ACOUSTID_MIN_SCORE).Result;

            // Output -> AcoustID Results
            outputItem.AcoustIDResults = acoustIDResults;
            outputItem.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();
        }
        private void GetMusicBrainzDetail()
        {
            // Output -> Music Brainz Recording Matches (heavy load)
            _workOutput.MusicBrainzRecordingMatches = _workOutput.AcoustIDResults
                                                                 .OrderByDescending(x => x.Score)
                                                                 .SelectMany(x => x.Recordings)
                                                                 .Select(x => _musicBrainzClient.GetRecordingById(new Guid(x.Id)).Result)
                                                                 .Where(x => _modelValidationService.ValidateMusicBrainzRecording_ImportBasic(x))
                                                                 .ToList();
            // Validate Results
            _workOutput.MusicBrainzRecordingMatchSuccess = _workOutput.MusicBrainzRecordingMatches.Any();
        }
        public void CalculateFileName()
        {
            // All groupings are based on the destination file folder (base)
            var fileName = Path.GetFileName(_workLoad.SourceFile);
            var calculatedFileName = fileName;

            switch (_workLoad.NamingType)
            {
                case LibraryEntryNamingType.None:
                    // Calculated file name is the original file name
                    break;
                case LibraryEntryNamingType.Standard:
                {
                    var format = "{0:#} {1}.mp3";
                    var formattedTitle = string.Format(format, _workOutput.ImportedTagFile.Tag.Track, _workOutput.ImportedTagFile.Tag.Title);
                    calculatedFileName = StringHelpers.MakeFriendlyPath(true, formattedTitle);
                }
                break;
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} {1}-{2}-{3}.mp3";
                    var formattedTitle = string.Format(format, _workOutput.ImportedTagFile.Tag.Track, _workOutput.ImportedTagFile.Tag.FirstGenre, _workOutput.ImportedTagFile.Tag.FirstAlbumArtist, _workOutput.ImportedTagFile.Tag.Track);
                    calculatedFileName = StringHelpers.MakeFriendlyPath(true, formattedTitle);
                }
                break;
                default:
                    throw new Exception("Unhandled naming type:  LibraryLoaderImportWorker.cs");
            }

            switch (_workLoad.GroupingType)
            {
                case LibraryEntryGroupingType.None:
                    _workOutput.DestinationPathCalculated = Path.Combine(_workLoad.DestinationFolder, calculatedFileName);
                    break;
                case LibraryEntryGroupingType.ArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(false, _workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(false, _workOutput.ImportedTagFile.Tag.Album);

                    _workOutput.DestinationSubFolders = new string[] { artistFolder, Path.Combine(artistFolder, albumFolder) };
                    _workOutput.DestinationPathCalculated = Path.Combine(_workLoad.DestinationFolder,
                                                                        artistFolder,
                                                                        albumFolder,
                                                                        calculatedFileName);
                }
                break;
                case LibraryEntryGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(false, _workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(false, _workOutput.ImportedTagFile.Tag.Album);
                    var genreFolder = StringHelpers.MakeFriendlyPath(false, _workOutput.ImportedTagFile.Tag.FirstGenre);

                    _workOutput.DestinationSubFolders = new string[] { genreFolder,
                                                                      Path.Combine(genreFolder, artistFolder),
                                                                      Path.Combine(genreFolder, artistFolder, albumFolder)};

                    _workOutput.DestinationPathCalculated = Path.Combine(_workLoad.DestinationFolder,
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
        public void ReadTagData()
        {
            if (string.IsNullOrEmpty(_workLoad.SourceFile))
                throw new ArgumentException("Invalid media file name");

            // File load parameters
            _workOutput.ImportedTagFileAvailable = Path.Exists(_workLoad.SourceFile);
            _workOutput.ImportedTagFileLoadError = false;
            _workOutput.ImportedTagFileErrorMessage = string.Empty;

            TagLib.File fileRef = null;

            try
            {
                fileRef = _tagCacheController.Get(_workLoad.SourceFile);

                if (fileRef == null)
                {
                    _workOutput.ImportedTagFileErrorMessage = "Unable to load tag from file";
                    _workOutput.ImportedTagFileLoadError = true;
                }

                _workOutput.ImportedTagFile = fileRef;
            }
            catch (Exception ex)
            {
                _workOutput.ImportedTagFileErrorMessage = ex.Message;
                _workOutput.ImportedTagFileLoadError = true;
            }
        }
        private void EmbedTagData()
        {
            if (string.IsNullOrEmpty(_workLoad.SourceFile))
                throw new ArgumentException("Invalid media file name");

            if (!_workOutput.MusicBrainzRecordingMatches.Any())
                throw new ArgumentException("No matching results were found. Embedding of tag data halted.");

            try
            {
                _workOutput.ImportedTagFileAvailable = System.IO.File.Exists(_workLoad.SourceFile);
                _workOutput.ImportedTagFileLoadError = false;
                _workOutput.ImportedTagFileErrorMessage = string.Empty;

                var fileRef = _tagCacheController.Get(_workLoad.SourceFile);

                if (fileRef == null)
                    return;

                var bestMatch = _workOutput.MusicBrainzRecordingMatches.First();

                // Validated Fields
                var release = bestMatch.Releases.First();

                fileRef.Tag.Album = release.Title;
                fileRef.Tag.AlbumSort = release.Title;
                fileRef.Tag.Artists = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
                fileRef.Tag.AlbumArtists = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
                fileRef.Tag.AlbumArtistsSort = bestMatch.ArtistCredit.Select(x => x.Name).ToArray();
                fileRef.Tag.Title = bestMatch.Title;
                fileRef.Tag.TitleSort = bestMatch.Title;                

                // Also Validated: This may be the CD or "Media" that contains the track (only); or some duplicate record of
                //                 that media (if there are multiple releases); but there will be at least one with the track information.
                //
                var trackMedia = release.Media.First(x => x.Tracks.Any(z => z.Title == bestMatch.Title));

                fileRef.Tag.Year = (uint)(release.Date.Year ?? 0);
                fileRef.Tag.Track = (uint)(trackMedia.Tracks.First(x => x.Title == bestMatch.Title).Position);
                fileRef.Tag.TrackCount = (uint)trackMedia.TrackCount;
                fileRef.Tag.Disc = (uint)(release.Media.IndexOf(trackMedia) + 1);
                fileRef.Tag.DiscCount = (uint)release.Media.Count;

                fileRef.Save();

                // Set Output
                _workOutput.ImportedTagFile = fileRef;
            }
            catch (Exception ex)
            {
                _workOutput.ImportedTagFileAvailable = true;
                _workOutput.ImportedTagFileLoadError = true;
                _workOutput.ImportedTagFileErrorMessage = ex.Message;
            }
        }
        private bool MoveFile(out string message)
        {
            try
            {
                message = string.Empty;

                // Make sure that sub-folders are in place
                if (!Directory.Exists(_workLoad.DestinationFolder))
                {
                    message = "Destination folder does not exist! Make sure that the music / audio books sub-folders have been created before executing import";
                    return false;
                }

                foreach (var subFolder in _workOutput.DestinationSubFolders)
                {
                    var directory = Path.Combine(_workLoad.DestinationFolder, subFolder);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                // Verify that calculated path now is available
                var fullDirectory = Path.GetDirectoryName(_workOutput.DestinationPathCalculated);

                // This should be here; but we can try using System.IO to create the directory 
                // one last time (before getting the exception)
                //
                if (!Directory.Exists(fullDirectory))
                    Directory.CreateDirectory(fullDirectory);

                System.IO.File.Move(_workLoad.SourceFile, _workOutput.DestinationPathCalculated, true);

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
