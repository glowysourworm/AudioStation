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

using TagLib.Ape;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderImportStagedFilesWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IAcoustIDClient _acoustIDClient;

        private const int MUSIC_BRAINZ_MIN_SCORE = 100;

        public LibraryLoaderImportStagedFilesWorker(IModelController modelController, 
                                                    IAcoustIDClient acoustIDClient)
        {
            _modelController = modelController;
            _acoustIDClient = acoustIDClient;
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
                var acoustIDResults = _acoustIDClient.IdentifyFingerprint(file, MUSIC_BRAINZ_MIN_SCORE).Result;
                var success = false;

                foreach (var item in acoustIDResults)
                {
                    // Lets log the result to see, then take the first one
                    ApplicationHelpers.Log("AcoustID result ({0}) {1}/{2}/{3} for {4}", 
                                           LogMessageType.Vendor, LogLevel.Information, MUSIC_BRAINZ_MIN_SCORE, item.Title, 
                                           item.Artists?.FirstOrDefault()?.Name ?? "(Unknown)", 
                                           item.Releases?.FirstOrDefault()?.Title ?? "(Unknown)");
                }

                var bestMatch = acoustIDResults.Where(x => !string.IsNullOrEmpty(x.Title) &&
                                                           x.Artists.Any(artist => !string.IsNullOrEmpty(artist.Name)) &&
                                                           x.Releases.Any(release => !string.IsNullOrEmpty(release.Title))).FirstOrDefault();

                // Success (Call / Cache MusicBrainz)
                if (bestMatch != null)
                {
                    var artist = bestMatch.Artists.First();
                    var release = bestMatch.Releases.First();
                    var trackName = bestMatch.Title;

                    // Query / Cache MusicBrainz
                    var recordMatches = _modelController.GetCompleteMusicBrainzRecord(artist.Name, release.Title, trackName);

                    // Lookup record with matching ID
                    var record = recordMatches.FirstOrDefault(x => x.Recording.Id.ToString() == bestMatch.Id);
                   
                    if (record != null)
                    {
                        success = EmbedTagData(workItem.GetId(), file, record);
                    }

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
                                 MusicBrainzCombinedLibraryEntryRecord record)
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

                // COVER ART!
                //fileRef.Tag.Pictures

                fileRef.Tag.Title = record.Track.Title;
                fileRef.Tag.Track = (uint)(record.Track.Position ?? 0);
                fileRef.Tag.TrackCount = (uint)(record.Medium.TrackCount);
                fileRef.Tag.Year = (uint)(record.Release.Date.Value.Year);

                //fileRef.Tag.Disc = 

                return true;
            }
            catch (Exception ex)
            {
                //fileErrorMessage = ex.Message;
                //fileLoadError = true;

                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
                return false;
            }
        }
    }
}
