using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;

using TagLib;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderImportWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAcoustIDClient _acoustIDClient;

        private const int ACOUSTID_MIN_SCORE = 80;
        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        public LibraryLoaderImportWorker(IModelController modelController,
                                                    IAcoustIDClient acoustIDClient,
                                                    IMusicBrainzClient musicBrainzClient)
        {
            _modelController = modelController;
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient = musicBrainzClient;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var generalError = false;
            var fileAvailable = false;
            var fileLoadError = false;
            var fileErrorMessage = string.Empty;
            TagLib.File taglibRef = null;

            var importLoad = workItem.GetWorkItem() as LibraryLoaderImportLoad;
            var outputItem = workItem.GetOutputItem() as LibraryLoaderImportLoadOutput;

            // Processing...
            workItem.Start();

            // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
            //          get; and hope that it works right out of the box.
            //
            var acoustIDResults = _acoustIDClient.IdentifyFingerprint(importLoad.SourcePath, ACOUSTID_MIN_SCORE).Result;
            var success = false;

            // Output -> AcoustID Results
            outputItem.AcoustIDResults = acoustIDResults;
            outputItem.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();

            foreach (var item in acoustIDResults)
            {
                var resultFormat = "AcoustID Result: Id={0} MinScore={1}, Score={2}, Title={3}";

                foreach (var recording in item.Recordings)
                {
                    // Lets log the result to see, then take the first one
                    outputItem.Log.Add(new LogMessage(string.Format(item.Id, MUSIC_BRAINZ_MIN_SCORE, item.Score, recording.Title), LogMessageType.LibraryLoader));
                }
            }

            /*
                AcoustID:  Their results have detailed data (sometimes). I'm seeing that the release group is usually where
                           to get the MBID.
            */

            //var bestMatch = acoustIDResults.Where(x => !string.IsNullOrEmpty(x.Title) &&
            //                                           x.Artists.Any(artist => !string.IsNullOrEmpty(artist.Name)) &&
            //                                           x.Releases.Any(release => !string.IsNullOrEmpty(release.Title))).FirstOrDefault();

            var bestMatches = acoustIDResults.Select(x => _musicBrainzClient.GetRecordingById(new Guid(x.Id)).Result);
            var bestMatch = bestMatches.FirstOrDefault();

            // Output -> Music Brainz Recording Matches
            outputItem.MusicBrainzRecordingMatches = bestMatches;

            if (bestMatch != null &&
                bestMatch.ArtistCredit != null &&
                bestMatch.ArtistCredit.Any() &&
                bestMatch.Releases != null &&
                bestMatch.Releases.Any() &&
               !string.IsNullOrEmpty(bestMatch.ArtistCredit.First().Name) &&
               !string.IsNullOrEmpty(bestMatch.Releases.First().Title) &&
               !string.IsNullOrEmpty(bestMatch.Title))
            {
                // Music Brainz Match Complete
                outputItem.MusicBrainzRecordingMatchSuccess = true;

                // Success (Call / Cache MusicBrainz)
                var recordMatches = _modelController.GetCompleteMusicBrainzRecord(bestMatch.ArtistCredit.First().Name,
                                                                                  bestMatch.Releases.First().Title,
                                                                                  bestMatch.Title);

                // Output -> Music Brainz Combined Query Records
                outputItem.MusicBrainzCombinedRecordQuerySuccess = recordMatches != null && recordMatches.Any();
                outputItem.MusicBrainzCombinedLibraryEntryRecords = recordMatches;

                if (recordMatches.Any())
                {
                    // Lookup record with matching ID (try and select greedily for the artwork)
                    //
                    var record = recordMatches.FirstOrDefault(x => x.Track.Title == bestMatch.Title);
                    var artwork = recordMatches.SelectMany(x => x.ReleasePictures).ToList();

                    var front = record.ReleasePictures.Any(x => x.Type == PictureType.FrontCover) ?
                                record.ReleasePictures.First(x => x.Type == PictureType.FrontCover) :
                                artwork.FirstOrDefault(x => x.Type == PictureType.FrontCover);

                    var back = record.ReleasePictures.Any(x => x.Type == PictureType.BackCover) ?
                               record.ReleasePictures.First(x => x.Type == PictureType.BackCover) :
                               artwork.FirstOrDefault(x => x.Type == PictureType.BackCover);

                    if (record != null)
                    {
                        // Music Brainz Combined Record Query Complete
                        outputItem.MusicBrainzCombinedRecordQuerySuccess = true;
                        outputItem.FinalQueryRecord = record;

                        taglibRef = EmbedTagData(workItem.GetId(), importLoad.SourcePath, record, front, back, out fileAvailable, out fileLoadError, out fileErrorMessage);

                        success = taglibRef != null;

                        // Tag Embedding Complete
                        outputItem.TagEmbeddingSuccess = success;
                    }
                }

                // Failure
                else
                {
                    success = false;
                }
            }

            // Failure
            else
            {
                success = false;
            }

            // Import Record
            if (success)
            {
                // Save imported entity for output
                outputItem.ImportedRecord = _modelController.AddUpdateLibraryEntry(importLoad.SourcePath, fileAvailable, fileLoadError, fileErrorMessage, taglibRef);
                outputItem.Mp3FileImportSuccess = outputItem.ImportedRecord != null;

                // Set success flag
                success = outputItem.ImportedRecord != null;
            }

            // Move File
            if (success)
            {
                var outputPathFinal = string.Empty;

                // Move File, and update the entity (database)
                outputItem.Mp3FileMoveSuccess = MoveFile(workItem.GetId(), outputItem.ImportedRecord, outputItem.DestinationFolderBase, out outputPathFinal);
                outputItem.DestinationPathCalculated = outputPathFinal;

                success = outputItem.Mp3FileMoveSuccess;
            }

            // TODO: Move the file to it's permanent location in the audio library
            importLoad.Success = success;

            // Report -> UI Dispatcher (progress update)
            //
            Report(new LibraryWorkItem()
            {
                Id = workItem.GetId(),
                HasErrors = workItem.GetHasErrors(),
                LoadState = workItem.GetLoadState(),
                LoadType = workItem.GetLoadType(),
                FailureCount = workItem.GetFailureCount(),
                SuccessCount = workItem.GetSuccessCount(),
                EstimatedCompletionTime = DateTime.Now.AddSeconds(DateTime.Now.Subtract(workItem.GetStartTime()).TotalSeconds / (workItem.GetPercentComplete() == 0 ? 1 : workItem.GetPercentComplete())),
                PercentComplete = workItem.GetPercentComplete(),
                LastMessage = importLoad.SourcePath
            });

            if (generalError)
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public TagLib.File LoadLibraryEntry(int workItemId, string file, out string fileErrorMessage, out bool fileAvailable, out bool fileLoadError)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            // File load parameters
            fileAvailable = Path.Exists(file);
            fileErrorMessage = "";
            fileLoadError = false;

            TagLib.File fileRef = null;

            try
            {
                fileRef = TagLib.File.Create(file);

                if (fileRef == null)
                {
                    fileErrorMessage = "Unable to load tag from file";
                    fileLoadError = true;
                }

                return fileRef;
            }
            catch (Exception ex)
            {
                fileErrorMessage = ex.Message;
                fileLoadError = true;

                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private TagLib.File EmbedTagData(int workItemId,
                                         string fileName,
                                         MusicBrainzCombinedLibraryEntryRecord record,
                                         MusicBrainzPicture? frontCover,
                                         MusicBrainzPicture? backCover,
                                         out bool fileAvailable,
                                         out bool fileLoadError,
                                         out string fileErrorMessage)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid media file name");

            try
            {
                fileAvailable = System.IO.File.Exists(fileName);
                fileLoadError = false;
                fileErrorMessage = string.Empty;

                var fileRef = TagLib.File.Create(fileName);

                if (fileRef == null)
                    return null;

                fileRef.Tag.Album = record.Release.Title;
                fileRef.Tag.AlbumArtists = record.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.AlbumArtistsSort = record.Artists.Select(x => x.SortName).ToArray();
                fileRef.Tag.AmazonId = record.Release.Asin;
                fileRef.Tag.Artists = record.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.Comment = record.Recording.Annotation;
                fileRef.Tag.Disc = (uint)(record.Release.Media.IndexOf(x => x.Id == record.Medium.Id) + 1);
                fileRef.Tag.DiscCount = (uint)record.Release.Media.Count;
                fileRef.Tag.Genres = record.RecordingGenres.Select(x => x.Name).ToArray();
                fileRef.Tag.MusicBrainzArtistId = record.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseArtistId = record.Artists.FirstOrDefault()?.Id.ToString() ?? string.Empty;
                fileRef.Tag.MusicBrainzReleaseCountry = record.Release.Country;
                fileRef.Tag.MusicBrainzReleaseId = record.Release.Id.ToString();
                fileRef.Tag.MusicBrainzReleaseStatus = record.Release.Status;
                fileRef.Tag.MusicBrainzTrackId = record.Track.Id.ToString();
                fileRef.Tag.Performers = record.Artists.Select(x => x.Name).ToArray();
                fileRef.Tag.PerformersSort = record.Artists.Select(x => x.SortName).ToArray();

                var pictures = new List<IPicture>();

                if (frontCover != null)
                    pictures.Add(frontCover);

                if (backCover != null)
                    pictures.Add(backCover);

                // COVER ART!
                fileRef.Tag.Pictures = pictures.ToArray();

                fileRef.Tag.Title = record.Track.Title;
                fileRef.Tag.Track = (uint)(record.Track.Position ?? 0);
                fileRef.Tag.TrackCount = (uint)record.Medium.TrackCount;
                fileRef.Tag.Year = (uint)record.Release.Date.Value.Year;

                fileRef.Save();

                return fileRef;
            }
            catch (Exception ex)
            {
                fileAvailable = true;
                fileLoadError = true;
                fileErrorMessage = ex.Message;

                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
                return null;
            }
        }

        private bool MoveFile(int workItemId, Mp3FileReference importedEntity, string baseFolder, out string outputPath)
        {
            outputPath = string.Empty;

            try
            {
                var artistFolder = StringHelpers.MakeFriendlyPath(importedEntity?.PrimaryArtist?.Name ?? string.Empty);
                var albumFolder = StringHelpers.MakeFriendlyPath(importedEntity?.Album?.Name ?? string.Empty);
                var fileName = StringHelpers.MakeFriendlyFileName(importedEntity?.Title ?? string.Empty);

                // Final Path
                outputPath = Path.Combine(baseFolder, artistFolder, albumFolder, fileName);

                importedEntity.FileName = outputPath;

                // Move File
                System.IO.File.Move(importedEntity.FileName, outputPath, true);

                // Update Database (file name changed)
                return _modelController.UpdateAudioStationEntity(importedEntity);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file move error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
                return false;
            }
        }
    }
}
