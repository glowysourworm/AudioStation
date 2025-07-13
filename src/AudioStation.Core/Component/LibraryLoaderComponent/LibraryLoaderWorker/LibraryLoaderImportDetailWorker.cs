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
    public class LibraryLoaderImportDetailWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly ITagCacheController _tagCacheController;
        private readonly IModelValidationService _modelValidationService;

        private const int WORK_STEPS = 6;
        private const int ACOUSTID_MIN_SCORE = 80;
        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        private LibraryLoaderImportDetailLoad _workLoad;
        private LibraryLoaderImportDetailOutput _workOutput;

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderImportDetailWorker(LibraryLoaderWorkItem workItem,
                                         IModelController modelController,
                                         IAcoustIDClient acoustIDClient,
                                         IMusicBrainzClient musicBrainzClient,
                                         ITagCacheController tagCacheController,
                                         IModelValidationService modelValidationService)
            : base(workItem)
        {
            _workLoad = workItem.GetWorkItem() as LibraryLoaderImportDetailLoad;
            _workOutput = workItem.GetOutputItem() as LibraryLoaderImportDetailOutput;

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
                    var success = WorkMusicBrainzCompleteRecord(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 4:
                {
                    var message = string.Empty;
                    var success = WorkEmbedTag(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 5:
                {
                    var message = string.Empty;
                    var success = WorkImportEntity(out message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 6:
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
        private bool WorkMusicBrainzCompleteRecord(out string message)
        {
            if (_workOutput.MusicBrainzCombinedRecordQuerySuccess)
            {
                message = "Music Brainz combined record (final) results already retrieved successfully";
                return true;
            }

            GetMusicBrainzCompletedRecord();

            // Music Brainz Detail Result
            var success = _workOutput.MusicBrainzCombinedRecordQuerySuccess;

            message = success ? "Music Brainz (final completed record) lookup successful" : "Music Brainz (final completed record) lookup failed:  please see results";

            return success;
        }
        private bool WorkEmbedTag(out string message)
        {
            // Embed tag data into the SOURCE FILE
            EmbedTagData();

            var success = _workOutput.ImportedTagFileLoadError || !_workOutput.ImportedTagFileAvailable || _workOutput.ImportedTagFile == null;

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
            _workOutput.Mp3FileMoveSuccess = MoveFile();

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
                                                                 .ToList();
            // Validate Results
            _workOutput.MusicBrainzRecordingMatchSuccess = _workOutput.MusicBrainzRecordingMatches.Any(x => _modelValidationService.ValidateMusicBrainzRecording_ImportBasic(x));
        }
        public void GetMusicBrainzCompletedRecord()
        {
            // Take the first result - the rest may be presented to the user for further processing
            var bestMatch = _workOutput.MusicBrainzRecordingMatches
                                       .FirstOrDefault(x => _modelValidationService.ValidateMusicBrainzRecording_ImportBasic(x));

            if (bestMatch != null)
            {
                // Success (Call / Cache MusicBrainz) (heavy load)
                var recordMatches = _modelController.GetCompleteMusicBrainzRecord(bestMatch.ArtistCredit.First().Name,
                                                                                  bestMatch.Releases.First().Title,
                                                                                  bestMatch.Title);

                // Output -> Music Brainz Combined Query Records
                _workOutput.MusicBrainzCombinedRecordQuerySuccess = recordMatches != null && recordMatches.Any();
                _workOutput.MusicBrainzCombinedLibraryEntryRecords = recordMatches;

                if (recordMatches.Any())
                {
                    // Lookup record with matching ID (try and select greedily for the artwork)
                    //
                    var record = recordMatches.FirstOrDefault(x => x.Track.Title == bestMatch.Title);

                    if (record != null)
                    {
                        var artwork = recordMatches.SelectMany(x => x.ReleasePictures).ToList();

                        // Scrape any artwork that is already downloaded from the results
                        var front = record.ReleasePictures.Any(x => x.Type == PictureType.FrontCover) ?
                                    record.ReleasePictures.First(x => x.Type == PictureType.FrontCover) :
                                    artwork.FirstOrDefault(x => x.Type == PictureType.FrontCover);

                        var back = record.ReleasePictures.Any(x => x.Type == PictureType.BackCover) ?
                                   record.ReleasePictures.First(x => x.Type == PictureType.BackCover) :
                                   artwork.FirstOrDefault(x => x.Type == PictureType.BackCover);

                        // Store artwork
                        _workOutput.BestBackCover = back;
                        _workOutput.BestFrontCover = front;

                        // Music Brainz Combined Record Query Complete
                        _workOutput.MusicBrainzCombinedRecordQuerySuccess = true;
                        _workOutput.FinalQueryRecord = record;
                    }
                }
            }
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
                    var format = "{0:#} {1}-{2}.mp3";
                    var formattedTitle = string.Format(format, _workOutput.ImportedTagFile.Tag.Track, _workOutput.ImportedTagFile.Tag.FirstAlbumArtist, _workOutput.ImportedTagFile.Tag.Track);
                    calculatedFileName = StringHelpers.MakeFriendlyFileName(formattedTitle);
                }
                break;
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} {1}-{2}-{3}.mp3";
                    var formattedTitle = string.Format(format, _workOutput.ImportedTagFile.Tag.Track, _workOutput.ImportedTagFile.Tag.FirstGenre, _workOutput.ImportedTagFile.Tag.FirstAlbumArtist, _workOutput.ImportedTagFile.Tag.Track);
                    calculatedFileName = StringHelpers.MakeFriendlyFileName(formattedTitle);
                }
                break;
                default:
                    throw new Exception("Unhandled naming type:  LibraryLoaderImportWorker.cs");
            }

            switch (_workLoad.GroupingType)
            {
                case LibraryEntryGroupingType.None:
                    _workOutput.DestinationPathCalculated = Path.Combine(_workOutput.DestinationFolderBase, calculatedFileName);
                    break;
                case LibraryEntryGroupingType.ArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(_workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(_workOutput.ImportedTagFile.Tag.Album);

                    _workOutput.DestinationSubFolders = new string[] { artistFolder, Path.Combine(artistFolder, albumFolder) };
                    _workOutput.DestinationPathCalculated = Path.Combine(_workOutput.DestinationFolderBase,
                                                                        artistFolder,
                                                                        albumFolder,
                                                                        calculatedFileName);
                }
                break;
                case LibraryEntryGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(_workOutput.ImportedTagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(_workOutput.ImportedTagFile.Tag.Album);
                    var genreFolder = StringHelpers.MakeFriendlyPath(_workOutput.ImportedTagFile.Tag.FirstGenre);

                    _workOutput.DestinationSubFolders = new string[] { genreFolder,
                                                                      Path.Combine(genreFolder, artistFolder),
                                                                      Path.Combine(genreFolder, artistFolder, albumFolder)};

                    _workOutput.DestinationPathCalculated = Path.Combine(_workOutput.DestinationFolderBase,
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

            if (_workOutput.FinalQueryRecord == null)
                throw new ArgumentException("Final query record not yet completed. Embedding of tag data halted.");

            try
            {
                _workOutput.ImportedTagFileAvailable = System.IO.File.Exists(_workLoad.SourceFile);
                _workOutput.ImportedTagFileLoadError = false;
                _workOutput.ImportedTagFileErrorMessage = string.Empty;

                var fileRef = _tagCacheController.Get(_workLoad.SourceFile);

                if (fileRef == null)
                    return;

                fileRef.Tag.Album = _workOutput.FinalQueryRecord.Release.Title;
                fileRef.Tag.AlbumArtists = _workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.AlbumArtistsSort = _workOutput.FinalQueryRecord.Artists.Select(x => x.SortName).ToArray();
                fileRef.Tag.AmazonId = _workOutput.FinalQueryRecord.Release.Asin;
                fileRef.Tag.Artists = _workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.Comment = _workOutput.FinalQueryRecord.Recording.Annotation;
                fileRef.Tag.Disc = (uint)(_workOutput.FinalQueryRecord.Release.Media.IndexOf(x => x.Id == _workOutput.FinalQueryRecord.Medium.Id) + 1);
                fileRef.Tag.DiscCount = (uint)_workOutput.FinalQueryRecord.Release.Media.Count;
                fileRef.Tag.Genres = _workOutput.FinalQueryRecord.RecordingGenres.Select(x => x.Name).ToArray();
                fileRef.Tag.MusicBrainzArtistId = _workOutput.FinalQueryRecord.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseArtistId = _workOutput.FinalQueryRecord.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseCountry = _workOutput.FinalQueryRecord.Release.Country;
                fileRef.Tag.MusicBrainzReleaseId = _workOutput.FinalQueryRecord.Release.Id.ToString();
                fileRef.Tag.MusicBrainzReleaseStatus = _workOutput.FinalQueryRecord.Release.Status;
                fileRef.Tag.MusicBrainzTrackId = _workOutput.FinalQueryRecord.Track.Id.ToString();
                fileRef.Tag.Performers = _workOutput.FinalQueryRecord.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.PerformersSort = _workOutput.FinalQueryRecord.Artists.Select(x => x.SortName).ToArray();

                var pictures = new List<IPicture>();

                if (_workOutput.BestFrontCover != null)
                    pictures.Add(_workOutput.BestFrontCover);

                if (_workOutput.BestBackCover != null)
                    pictures.Add(_workOutput.BestBackCover);

                // COVER ART!
                fileRef.Tag.Pictures = pictures.ToArray();

                fileRef.Tag.Title = _workOutput.FinalQueryRecord.Track.Title;
                fileRef.Tag.Track = (uint)(_workOutput.FinalQueryRecord.Track.Position ?? 0);
                fileRef.Tag.TrackCount = (uint)_workOutput.FinalQueryRecord.Medium.TrackCount;
                fileRef.Tag.Year = (uint)(_workOutput.FinalQueryRecord.Release?.Date?.Year ?? 0);

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
        private bool MoveFile()
        {
            try
            {
                System.IO.File.Move(_workLoad.SourceFile, _workOutput.DestinationPathCalculated, true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
