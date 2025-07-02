using System.IO;

using AcoustID.Web;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.MusicBrainzDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;

using TagLib;
using TagLib.Ape;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderImportStagedFilesWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAcoustIDClient _acoustIDClient;

        private const int ACOUSTID_MIN_SCORE = 80;
        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        public LibraryLoaderImportStagedFilesWorker(IModelController modelController, 
                                                    IAcoustIDClient acoustIDClient,
                                                    IMusicBrainzClient musicBrainzClient)
        {
            _modelController = modelController;
            _acoustIDClient = acoustIDClient;
            _musicBrainzClient = musicBrainzClient;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var fileLoadError = false;
            var fileAvailable = false;
            var fileErrorMessasge = "";
            var generalError = false;

            var fileLoad = workItem.GetWorkItem() as LibraryLoaderFileLoad;

            // Processing...
            workItem.Start();

            // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
            //          get; and hope that it works right out of the box.
            //
            foreach (var file in fileLoad.GetPendingFiles())
            {                
                var acoustIDResults = _acoustIDClient.IdentifyFingerprint(file, ACOUSTID_MIN_SCORE).Result;
                var success = false;

                foreach (var item in acoustIDResults)
                {
                    // Lets log the result to see, then take the first one
                    ApplicationHelpers.Log("AcoustID result ({0}) {1}/{2}/{3} for {4}",
                                           LogMessageType.Vendor, LogLevel.Information, 
                                           MUSIC_BRAINZ_MIN_SCORE, item.Title,
                                           item.Artists?.FirstOrDefault()?.Name ?? "(Unknown)",
                                           item.Releases?.FirstOrDefault()?.Title ?? "(Unknown)", file);
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

                if (bestMatch != null &&
                    bestMatch.ArtistCredit != null &&
                    bestMatch.ArtistCredit.Any() &&
                    bestMatch.Releases != null &&
                    bestMatch.Releases.Any() &&
                   !string.IsNullOrEmpty(bestMatch.ArtistCredit.First().Name) &&
                   !string.IsNullOrEmpty(bestMatch.Releases.First().Title) &&
                   !string.IsNullOrEmpty(bestMatch.Title))
                {
                    // Success (Call / Cache MusicBrainz)
                    var recordMatches = _modelController.GetCompleteMusicBrainzRecord(bestMatch.ArtistCredit.First().Name,
                                                                                      bestMatch.Releases.First().Title, 
                                                                                      bestMatch.Title);

                    if (recordMatches.Any())
                    {
                        // Lookup record with matching ID (try and select greedily for the artwork)
                        //
                        var record = recordMatches.FirstOrDefault(x => x.Track.Title == bestMatch.Title);
                        var artwork = recordMatches.SelectMany(x => x.ReleasePictures).ToList();

                        var front = record.ReleasePictures.Any(x => x.Type == TagLib.PictureType.FrontCover) ?
                                    record.ReleasePictures.First(x => x.Type == TagLib.PictureType.FrontCover) :
                                    artwork.FirstOrDefault(x => x.Type == TagLib.PictureType.FrontCover);

                        var back = record.ReleasePictures.Any(x => x.Type == TagLib.PictureType.BackCover) ?
                                   record.ReleasePictures.First(x => x.Type == TagLib.PictureType.BackCover) :
                                   artwork.FirstOrDefault(x => x.Type == TagLib.PictureType.BackCover);

                        if (record != null)
                        {
                            success = EmbedTagData(workItem.GetId(), file, record, front, back);
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

                // TODO: Move the file to it's permanent location in the audio library
                fileLoad.SetComplete(file, success);

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
                    LastMessage = file
                });
            }

            if (generalError)
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public bool EmbedTagData(int workItemId, 
                                 string fileName, 
                                 MusicBrainzCombinedLibraryEntryRecord record,
                                 MusicBrainzPicture? frontCover,
                                 MusicBrainzPicture? backCover)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid media file name");

            try
            {
                var fileRef = TagLib.File.Create(fileName);

                if (fileRef == null)
                    return false;

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
                fileRef.Tag.TrackCount = (uint)(record.Medium.TrackCount);
                fileRef.Tag.Year = (uint)(record.Release.Date.Value.Year);

                fileRef.Save();

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
                return false;
            }
        }
    }
}
