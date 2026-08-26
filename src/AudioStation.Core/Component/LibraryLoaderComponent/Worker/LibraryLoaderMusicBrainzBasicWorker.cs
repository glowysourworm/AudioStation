using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Vendor.Interface;

using IF.Lastfm.Core.Api.Helpers;

using SimpleWpf.Utilities;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzBasicWorker : LibraryLoaderWorker
    {
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private const int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzBasicWorker(
                IMusicBrainzClient musicBrainzClient,
                IAudioStationDbClient audioStationDbClient,
                LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _musicBrainzClient = musicBrainzClient;
            _audioStationDbClient = audioStationDbClient;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }
        public static int GetNumberSteps()
        {
            return WORK_STEPS;
        }

        protected override bool Work(int step, ref string message)
        {
            // Steps: (AcoustID was used to get MusicBrainz IRecording)
            //
            // 1) Music Brainz
            // 2) Database Import AcoustID Entit(y|ies)
            // 3) Album Art
            // 

            switch (step)
            {
                case 1:
                    return WorkMusicBrainzStep(ref message);
                case 2:
                    return WorkDbStep(ref message);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkMusicBrainzStep(ref string message)
        {
            try
            {
                var load = this.Load.Get<LibraryLoaderEntitySetLoad<AcoustIDLookupResult>>();

                foreach (var entity in load.EntitySet)
                {
                    // TODO: CLEAN THIS UP AS PART OF THE SERVICE MODEL. 
                    //
                    //       The MusicBrainz server was throwing it back at us for
                    //       hitting them too quickly. We need a throttle limit to
                    //       be part of the service architecture. So, there would
                    //       be a simple wait loop for every public call to their 
                    //       servers determined by the configuration.
                    //
                    Thread.Sleep(1500);

                    Log("Music Brainz client lookup started:  " + entity.FileName);

                    var musicBrainzResult = _musicBrainzClient.GetTagSmall(new AudioStationTagServiceModel(entity.MusicBrainzRecordingId));

                    if (musicBrainzResult != null)
                    {
                        this.Output.Get<LibraryLoaderEntitySetOutput<TagSmall>>().Add(BasicHelpers.Map<ITagSmall, TagSmall>(musicBrainzResult));

                        Log("Music Brainz client lookup finished:  " + entity.FileName);
                    }

                    else
                    {
                        Log("Music Brainz client lookup error:  " + entity.FileName);
                        return false;
                    }
                }

                message = "Music Brainz service successful";
                return true;
            }
            catch (Exception ex)
            {
                message = "Music Brainz service error: " + ex.Message;
                return false;
            }
        }

        private bool WorkDbStep(ref string message)
        {
            try
            {
                message = string.Empty;

                var updated = 0;
                var added = 0;
                var index = 0;

                var vendorName = VendorNames.MusicBrainz.GetAttribute<DisplayAttribute>().Name;
                var vendor = _audioStationDbClient.FirstEntity<Vendor>(x => x.VendorName == vendorName);

                if (vendor == null)
                {
                    message = "Failed to find 'Music Brainz' vendor in database. Please ensure that this vendor has been added to your configuration";
                    return false;
                }

                foreach (var result in this.Output.Get<LibraryLoaderEntitySetOutput<TagSmall>>().Entities)
                {
                    Log("Importing Music Brainz result to database:  " + result.Title);

                    var inputLoad = this.Load.Get<LibraryLoaderEntitySetLoad<AcoustIDLookupResult>>().EntitySet.ElementAt(index++);
                    var existingMap = _audioStationDbClient.FirstEntity<TagSmallVendorMap>(x => x.MusicBrainzRecordingId == inputLoad.MusicBrainzRecordingId);
                    var existingEntity = existingMap?.TagSmall;

                    // Update
                    if (existingEntity != null)
                    {
                        existingEntity.Album = result.Album;
                        existingEntity.AlbumArtist = result.AlbumArtist;
                        existingEntity.MediaNumber = result.MediaNumber;
                        existingEntity.MediaTotal = result.MediaTotal;
                        existingEntity.MediaFormat = result.MediaFormat;
                        existingEntity.DurationMilliseconds = result.DurationMilliseconds;
                        existingEntity.Year = result.Year;
                        existingEntity.Genre = result.Genre;
                        existingEntity.Title = result.Title;
                        existingEntity.TrackNumber = result.TrackNumber;
                        existingEntity.TrackTotal = result.TrackTotal;

                        _audioStationDbClient.UpdateEntity(existingEntity);

                        updated++;
                    }

                    // Add
                    else
                    {

                        // PostGres ID constraint (database will find these using the foreign keys)
                        result.Id = 0;

                        // Add -> Save -> assigns TagSmall.Id
                        _audioStationDbClient.AddEntity(result);

                        var resultMap = new TagSmallVendorMap()
                        {
                            Id = 0,
                            TagSmallId = result.Id,
                            VendorId = vendor.Id,
                            MusicBrainzRecordingId = inputLoad.MusicBrainzRecordingId
                        };

                        _audioStationDbClient.AddEntity(resultMap);

                        added++;
                    }

                    Log("Import Music Brainz result to database successful:  " + result.Title);
                }

                message = string.Format("Music Brainz results imported to database:  {0} added, {1} updated", added, updated);

                return true;
            }
            catch (Exception ex)
            {
                message = "Music Brainz database import error " + ex.Message;
                return false;
            }
        }
    }
}
