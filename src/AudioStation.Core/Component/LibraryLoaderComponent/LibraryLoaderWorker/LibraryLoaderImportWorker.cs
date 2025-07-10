using System.IO;

using AcoustID.Web;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

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
        private readonly ITagCacheController _tagCacheController;

        private const int ACOUSTID_MIN_SCORE = 80;
        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        public LibraryLoaderImportWorker(IModelController modelController,
                                         IAcoustIDClient acoustIDClient,
                                         IMusicBrainzClient musicBrainzClient,
                                         ITagCacheController tagCacheController)
        {
            _modelController = modelController;
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient = musicBrainzClient;
            _tagCacheController = tagCacheController;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            // Procedure
            //
            // - Using AcoustID / Music Brainz
            //      1) AcoustID
            //      2) Music Brainz Complete Record Detail
            //      3) Embed Tag File
            // 
            // - Using Existing Tag
            //      1) Read Tag
            //
            // Then:  Import Library Entry from tag data; and move (migrate) file.
            //
            //
            var importLoad = workItem.GetWorkItem() as LibraryLoaderImportLoad;
            var output = workItem.GetOutputItem() as LibraryLoaderImportLoadOutput;

            // Processing...
            workItem.Start();

            foreach (var sourceFile in importLoad.GetSourceFiles())
            {
                var outputItem = new LibraryLoaderImportFileResult();
                var success = true;
                var fileAvailable = false;
                var fileLoadError = false;
                var fileErrorMessage = string.Empty;
                TagLib.File tagFile = null;

                // Add result instance to output
                output.Results.Add(outputItem);

                // These have to be paired until we establish other ways of getting at the
                // MusicBrainz ID (which there are many). For now, these will be dual-checked.
                //
                if (importLoad.IdentifyUsingAcoustID && importLoad.IncludeMusicBrainzDetail)
                {
                    /*
                        AcoustID:  Their results have detailed data (sometimes). I'm seeing that the release group is usually where
                                   to get the MBID.
                    */
                    GetAcoustIDResults(ref outputItem, sourceFile);

                    // AcoustID Result
                    success &= outputItem.AcoustIDSuccess;

                    if (!success)
                    {
                        SendReport(workItem, "Acoust ID acoustic fingerprint identification failed:  please see results");
                        importLoad.SetResult(sourceFile, false);
                        continue;
                    }

                    GetMusicBrainzDetail(ref outputItem, sourceFile);

                    // Music Brainz Detail Result
                    success &= outputItem.MusicBrainzRecordingMatchSuccess && outputItem.MusicBrainzCombinedRecordQuerySuccess;

                    if (!success)
                    {
                        SendReport(workItem, "Music Brainz Lookup Failed:  please see results");
                        importLoad.SetResult(sourceFile, false);
                        continue;
                    }

                    // Embed tag data into the SOURCE FILE
                    tagFile = EmbedTagData(workItem.GetId(), sourceFile,
                                           outputItem.FinalQueryRecord,
                                           outputItem.BestFrontCover,
                                           outputItem.BestBackCover,
                                           out fileAvailable,
                                           out fileLoadError,
                                           out fileErrorMessage);
                }

                // Get tag data from the existing Mp3 file
                else
                {
                    tagFile = ReadTagData(workItem.GetId(), sourceFile, out fileErrorMessage, out fileAvailable, out fileLoadError);
                }

                // Make sure tag data was read before continuing
                success &= tagFile != null;

                if (!success)
                {
                    SendReport(workItem, "Tag data could not be located. Import progress halted:  please see results");
                    importLoad.SetResult(sourceFile, false);
                    continue;
                }

                // Calculate destination file name. Destination path is used no matter if there is a file migration or not.
                CalculateFileName(importLoad, sourceFile, tagFile, ref outputItem);

                // Import Record:  Save imported entity for output
                outputItem.ImportedRecord = _modelController.AddUpdateLibraryEntry(outputItem.DestinationPathCalculated, fileAvailable, fileLoadError, fileErrorMessage, tagFile);
                outputItem.Mp3FileImportSuccess = outputItem.ImportedRecord != null;

                // Update Result
                success &= outputItem.Mp3FileImportSuccess;

                if (!success)
                {
                    SendReport(workItem, "Final import of track data failed:  please see results");
                    importLoad.SetResult(sourceFile, false);
                    continue;
                }

                // Move File
                if (importLoad.ImportFileMigration)
                {
                    // Move File, and update the entity (database)
                    outputItem.Mp3FileMoveSuccess = MoveFile(workItem.GetId(), sourceFile, outputItem.DestinationPathCalculated);

                    // Update Result
                    success &= outputItem.Mp3FileMoveSuccess;

                    if (!success)
                    {
                        SendReport(workItem, "Final import succeeded; but the source file could not be migrated (move failed):  please see results");
                        importLoad.SetResult(sourceFile, false);
                        continue;
                    }
                }

                importLoad.SetResult(sourceFile, true);
            }

            if (workItem.GetHasErrors())
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public void SendReport(LibraryLoaderWorkItem workItem, string message)
        {
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
                LastMessage = message
            });
        }

        public void GetAcoustIDResults(ref LibraryLoaderImportFileResult outputItem, string fileFullName)
        {
            // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
            //          get; and hope that it works right out of the box.
            //
            var acoustIDResults = _acoustIDClient.IdentifyFingerprint(fileFullName, ACOUSTID_MIN_SCORE).Result;

            // Output -> AcoustID Results
            outputItem.AcoustIDResults = acoustIDResults;
            outputItem.AcoustIDSuccess = acoustIDResults != null && acoustIDResults.Any();
        }

        public void GetMusicBrainzDetail(ref LibraryLoaderImportFileResult outputItem, string fileFullName)
        {
            // Output -> Music Brainz Recording Matches (heavy load)
            outputItem.MusicBrainzRecordingMatches = outputItem.AcoustIDResults.Select(x => _musicBrainzClient.GetRecordingById(new Guid(x.Id)).Result);

            // Take the first result - the rest may be presented to the user for further processing
            var bestMatch = outputItem.MusicBrainzRecordingMatches.FirstOrDefault();

            if (bestMatch != null &&
                bestMatch.ArtistCredit != null &&
                bestMatch.ArtistCredit.Any() &&
                bestMatch.Releases != null &&
                bestMatch.Releases.Any() &&
               !string.IsNullOrEmpty(bestMatch.ArtistCredit.First().Name) &&
               !string.IsNullOrEmpty(bestMatch.Releases.First().Title) &&
               !string.IsNullOrEmpty(bestMatch.Title))
            {
                // Music Brainz Match Complete (VERIFIED MINIMAL RECORDS)
                outputItem.MusicBrainzRecordingMatchSuccess = true;

                // Success (Call / Cache MusicBrainz) (heavy load)
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
                        outputItem.BestBackCover = back;
                        outputItem.BestFrontCover = front;

                        // Music Brainz Combined Record Query Complete
                        outputItem.MusicBrainzCombinedRecordQuerySuccess = true;
                        outputItem.FinalQueryRecord = record;
                    }
                }
            }
        }
        
        public void CalculateFileName(LibraryLoaderImportLoad inputItem,
                                      string fileFullName, 
                                      TagLib.File tagFile,
                                      ref LibraryLoaderImportFileResult outputItem)
        {
            // All groupings are based on the destination file folder (base)
            var fileName = Path.GetFileName(fileFullName);
            var calculatedFileName = fileName;

            switch (inputItem.NamingType)
            {
                case LibraryEntryNamingType.None:
                    // Calculated file name is the original file name
                    break;
                case LibraryEntryNamingType.Standard:
                {
                    var format = "{0:#} {1}-{2}.mp3";
                    var formattedTitle = string.Format(format, tagFile.Tag.Track, tagFile.Tag.FirstAlbumArtist, tagFile.Tag.Track);
                    calculatedFileName = StringHelpers.MakeFriendlyFileName(formattedTitle);
                }    
                    break;
                case LibraryEntryNamingType.Descriptive:
                {
                    var format = "{0:#} {1}-{2}-{3}.mp3";
                    var formattedTitle = string.Format(format, tagFile.Tag.Track, tagFile.Tag.FirstGenre, tagFile.Tag.FirstAlbumArtist, tagFile.Tag.Track);
                    calculatedFileName = StringHelpers.MakeFriendlyFileName(formattedTitle);
                }
                break;
                default:
                    throw new Exception("Unhandled naming type:  LibraryLoaderImportWorker.cs");
            }

            switch (inputItem.GroupingType)
            {
                case LibraryEntryGroupingType.None:
                    outputItem.DestinationPathCalculated = Path.Combine(outputItem.DestinationFolderBase, calculatedFileName);
                    break;
                case LibraryEntryGroupingType.ArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(tagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(tagFile.Tag.Album);

                    outputItem.DestinationSubFolders = new string[] { artistFolder, Path.Combine(artistFolder, albumFolder) };
                    outputItem.DestinationPathCalculated = Path.Combine(outputItem.DestinationFolderBase, 
                                                                        artistFolder, 
                                                                        albumFolder, 
                                                                        calculatedFileName);
                }                    
                break;
                case LibraryEntryGroupingType.GenreArtistAlbum:
                {
                    var artistFolder = StringHelpers.MakeFriendlyPath(tagFile.Tag.FirstAlbumArtist);
                    var albumFolder = StringHelpers.MakeFriendlyPath(tagFile.Tag.Album);
                    var genreFolder = StringHelpers.MakeFriendlyPath(tagFile.Tag.FirstGenre);

                    outputItem.DestinationSubFolders = new string[] { genreFolder, 
                                                                      Path.Combine(genreFolder, artistFolder),
                                                                      Path.Combine(genreFolder, artistFolder, albumFolder)};

                    outputItem.DestinationPathCalculated = Path.Combine(outputItem.DestinationFolderBase,
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

        public TagLib.File ReadTagData(int workItemId, string file, out string fileErrorMessage, out bool fileAvailable, out bool fileLoadError)
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
                fileRef = _tagCacheController.Get(file);

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

                var fileRef = _tagCacheController.Get(fileName);

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

        private bool MoveFile(int workItemId, string sourceFilePath, string destinationFilePath)
        {
            try
            {
                System.IO.File.Move(sourceFilePath, destinationFilePath, true);
                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file move error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
                return false;
            }
        }
    }
}
