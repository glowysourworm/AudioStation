using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database;
using AudioStation.Core.Model;
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

            var entityLoad = workItem.GetWorkItem() as LibraryLoaderEntityLoad;

            // Processing...
            workItem.Start();

            foreach (var entity in entityLoad.GetPendingEntities().Cast<Mp3FileReference>())
            {
                string musicBrainzArtistId = string.Empty;
                string musicBrainzAlbumId = string.Empty;

                // Report -> UI Dispatcher (progress update)
                //
                Report(new LibraryWorkItem()
                {
                    Id = workItem.GetId(),
                    HasErrors = workItem.GetHasErrors(),
                    LoadState = workItem.GetLoadState(),
                    LoadType = workItem.GetLoadType(),
                    Runtime = DateTime.Now.Subtract(workItem.GetStartTime()),
                    PercentComplete = workItem.GetPercentComplete(),
                    LastMessage = "Loading Music Brainz Ids:  EntityId=" + entity.Id.ToString()
                });

                var recordings = _musicBrainzClient.Query(entity.PrimaryArtist?.Name ?? string.Empty,
                                                          entity.Album?.Name ?? string.Empty,
                                                          entity.Title ?? string.Empty, 100).Result;

                if (recordings == null)
                {
                    // Work Item:  Failure
                    generalError = true;
                    entityLoad.SetResult(entity, false);
                    continue;
                }

                var bestMatch = recordings.FirstOrDefault(x => x.Title == entity.Title &&
                                                               x.Releases != null &&
                                                               x.Releases.Count > 0 &&
                                                               x.Releases.Any(z => !string.IsNullOrEmpty(x.Title) && z.Title == entity.Album?.Name) &&
                                                               x.ArtistCredit != null &&
                                                               x.ArtistCredit.Count > 0 &&
                                                               x.ArtistCredit.Any(z => !string.IsNullOrEmpty(z.Name) && z.Name == entity.PrimaryArtist?.Name));

                if (bestMatch == null)
                {
                    // Work Item:  Failure
                    generalError = true;
                    entityLoad.SetResult(entity, false);
                    continue;
                }
                else
                {
                    // VALIDATION:  Must have matching artist / album / track names
                    //
                    if (bestMatch.Title != entity.Title ||
                        bestMatch.Releases == null ||
                        bestMatch.Releases.Count == 0 ||
                       !bestMatch.Releases.Any(x => !string.IsNullOrEmpty(x.Title) && x.Title == entity.Album?.Name) ||
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
                        var albumMusicBrainz = bestMatch.Releases.First(x => !string.IsNullOrEmpty(x.Title) && x.Title == entity.Album?.Name);

                        entity.MusicBrainzTrackId = trackId.ToString();

                        // Update Track Entity (the album / artist will be updated either here or on another work item)
                        _modelController.UpdateEntity(entity);

                        // Go ahead and fetch the other entities for the music brainz id's
                        var artistEntity = _modelController.GetEntity<Mp3FileReferenceArtist>(entity.PrimaryArtistId.Value);
                        var albumEntity = _modelController.GetEntity<Mp3FileReferenceAlbum>(entity.AlbumId.Value);

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
                        _modelController.UpdateEntity(artistEntity);
                        _modelController.UpdateEntity(albumEntity);
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

                ApplicationHelpers.LogSeparate(workItemId, "Mp3 file updated (w/ Music Brainz Ids):  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, file);

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
