using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderFillMusicBrainzIdsWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;
        private readonly IMusicBrainzClient _musicBrainzClient;

        public LibraryLoaderFillMusicBrainzIdsWorker(IModelController modelController,
                                                     IOutputController outputController,
                                                     IMusicBrainzClient musicBrainzClient) : base(outputController)
        {
            _modelController = modelController;
            _musicBrainzClient = musicBrainzClient;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var generalError = false;
            var musicBrainzMinScore = 100;      // Search Query "Match Score"

            var entityLoad = workItem.GetWorkItem() as LibraryLoaderEntityLoad;

            // Processing...
            workItem.Start();

            foreach (var entity in entityLoad.GetPendingEntities().Cast<Mp3FileReference>())
            {

                string musicBrainzArtistId = string.Empty;
                string musicBrainzAlbumId = string.Empty;

                MusicBrainzTrack? bestMatch = null;

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
                    LastMessage = "Loading Music Brainz Ids:  EntityId=" + entity.Id.ToString()
                });

                // Existing Music Brainz ID
                //
                if (!string.IsNullOrEmpty(entity.MusicBrainzTrackId))
                {
                    var track = _musicBrainzClient.GetTrack(new Guid(entity.MusicBrainzTrackId), 
                                                            entity.Title ?? string.Empty, 
                                                            entity.Album?.Name ?? string.Empty, 
                                                            entity.PrimaryArtist?.Name ?? string.Empty,
                                                            musicBrainzMinScore).Result;

                    // Go ahead and skip this one (there will be the artist / album tag updaters separately)
                    if (track != null &&
                        track.Title == entity.Title)
                    {
                        bestMatch = track;
                    }
                }               

                // Music Brainz Query
                //
                if (bestMatch == null)
                {
                    bestMatch = _musicBrainzClient.GetTrack(entity.Title ?? string.Empty, 
                                                            entity.Album?.Name ?? string.Empty, 
                                                            entity.PrimaryArtist?.Name ?? string.Empty,
                                                            musicBrainzMinScore).Result;
                }

                // General Error
                //
                if (bestMatch == null)
                {
                    // Work Item:  Failure
                    generalError = true;
                    entityLoad.SetResult(entity, false);
                    continue;
                }

                // Validation
                //
                else
                {
                    // Must have matching artist / album / track names
                    //
                    if (bestMatch.Title != entity.Title ||
                        bestMatch.Recording == null ||
                        bestMatch.Recording.Title != entity.Album?.Name ||
                        bestMatch.Recording.Releases == null ||
                        bestMatch.Recording.Releases.Count == 0 ||
                       !bestMatch.Recording.Releases.Any(x => !string.IsNullOrEmpty(x.Title) && x.Title == entity.Album?.Name) ||
                        bestMatch.ArtistCredit == null ||
                        bestMatch.ArtistCredit.Count == 0 ||
                       !bestMatch.ArtistCredit.Any(x => !string.IsNullOrEmpty(x.Name) &&
                                                         x.Name == entity.PrimaryArtist?.Name))
                    {
                        // Work Item:  Failure
                        generalError = true;
                        entityLoad.SetResult(entity, false);
                        continue;
                    }

                    // Success!
                    else
                    {
                        var trackId = bestMatch.Id;
                        var artistMusicBrainz = bestMatch.ArtistCredit.First(x => x.Name == entity.PrimaryArtist?.Name);
                        var albumMusicBrainz = bestMatch.Recording.Releases.First(x => !string.IsNullOrEmpty(x.Title) && x.Title == entity.Album?.Name);

                        entity.MusicBrainzTrackId = trackId.ToString();

                        // Update Track Entity (the album / artist will be updated either here or on another work item)
                        _modelController.UpdateAudioStationEntity(entity);

                        // Go ahead and fetch the other entities for the music brainz id's
                        var artistEntity = _modelController.GetAudioStationEntity<Mp3FileReferenceArtist>(entity.PrimaryArtistId.Value);
                        var albumEntity = _modelController.GetAudioStationEntity<Mp3FileReferenceAlbum>(entity.AlbumId.Value);

                        if (artistEntity == null)
                        {
                            // Work Item:  Failure
                            generalError = true;
                            entityLoad.SetResult(entity, false);
                            continue;
                        }
                        if (albumEntity == null)
                        {
                            // Work Item:  Failure
                            generalError = true;
                            entityLoad.SetResult(entity, false);
                            continue;
                        }

                        artistEntity.MusicBrainzArtistId = artistMusicBrainz.Artist?.Id.ToString();
                        albumEntity.MusicBrainzReleaseId = albumMusicBrainz.Id.ToString();

                        // Save these to the tag
                        musicBrainzArtistId = artistEntity.MusicBrainzArtistId;
                        musicBrainzAlbumId = albumEntity.MusicBrainzReleaseId;

                        // Update Entities
                        _modelController.UpdateAudioStationEntity(artistEntity);
                        _modelController.UpdateAudioStationEntity(albumEntity);
                    }
                }

                // Set Tag Data
                if (!SetMusicBrainzTagData(workItem.GetId(), entity.FileName, musicBrainzArtistId, musicBrainzAlbumId, entity.MusicBrainzTrackId))
                    entityLoad.SetResult(entity, false);

                // Work Item:  Success!
                else
                    entityLoad.SetResult(entity, true);

                ApplicationHelpers.LogSeparate(workItem.GetId(), "Music Brainz Id's Updated (database):  Mp3FileReference.Id={0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Information, entity.Id);
            }

            if (generalError)
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public bool SetMusicBrainzTagData(int workItemId, string file, string musicBrainzArtistId, string musicBrainzAlbumId, string musicBrainzTrackId)
        {
            TagLib.File fileRef = null;

            try
            {
                fileRef = TagLib.File.Create(file);

                if (fileRef == null)
                {
                    return false;
                }
                if (string.IsNullOrWhiteSpace(musicBrainzArtistId) ||
                    string.IsNullOrWhiteSpace(musicBrainzAlbumId) ||
                    string.IsNullOrWhiteSpace(musicBrainzTrackId))
                    return false;

                fileRef.Tag.MusicBrainzArtistId = musicBrainzArtistId;
                fileRef.Tag.MusicBrainzReleaseArtistId = musicBrainzArtistId;
                fileRef.Tag.MusicBrainzTrackId = musicBrainzTrackId;

                fileRef.Save();

                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file updated (w/ Music Brainz Ids):  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Information, file);
                ApplicationHelpers.Log("Mp3 file updated (w/ Music Brainz Ids):  {0}", LogMessageType.FileTagUpdate, LogLevel.Information, file);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
            }

            return false;
        }
    }
}
