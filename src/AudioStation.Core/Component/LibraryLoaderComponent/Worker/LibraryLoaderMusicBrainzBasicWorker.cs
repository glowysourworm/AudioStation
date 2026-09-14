using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Payload;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;

using IF.Lastfm.Core.Api.Helpers;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzBasicWorker : LibraryLoaderWorker
    {
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private const int WORK_STEPS = 2;

        public LibraryLoaderMusicBrainzBasicWorker(
                IAudioStationMapper audioStationMapper,
                IMusicBrainzClient musicBrainzClient,
                IAudioStationDbClient audioStationDbClient,
                LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _audioStationMapper = audioStationMapper;
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

        protected override LibraryWorkerStepResult Work(int step)
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
                    return WorkMusicBrainzStep(step);
                case 2:
                    return WorkDbStep(step);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkMusicBrainzStep(int stepNumber)
        {
            try
            {
                var load = this.Load.Get<LibraryLoaderEntitySetLoad<AcoustIDLookupResult>>();

                foreach (var entity in load.EntitySet)
                {
                    // Valid Entities Only (from AcoustID lookup)
                    if (entity.MusicBrainzRecordingId != null)
                    {

                        Log("Music Brainz client lookup started:  " + entity.FileName);

                        var response = _musicBrainzClient.ProcessRequest(new AudioStationTagServiceRequest(AudioStationTagRequestType.TagSmall, (Guid)entity.MusicBrainzRecordingId));
                        var result = (response.Payload as TagSmallPayload).Data;
                        var validation = TagValidator.ValidateTagSmallImport(result);

                        if (response.Success && validation.IsValid)
                        {
                            var tagSmall = _audioStationMapper.Map<ITagSmall, TagSmall>(result);

                            // Import Workflow
                            tagSmall.ImportWorkflowId = this.WorkflowId;

                            this.Output.Get<LibraryLoaderEntitySetOutput<TagSmall>>().Add(tagSmall);

                            Log("Music Brainz client lookup finished (valid):  " + entity.FileName);
                        }

                        else if (!validation.IsValid)
                        {
                            Log("Music Brainz client lookup skipped (invalid):  " + entity.FileName);
                            Log("Validation Message:  " + validation.ValidationMessage);

                            return new LibraryWorkerStepResult()
                            {
                                Completed = false,
                                Message = "Music Brainz lookup invalid: " + validation.ValidationMessage,
                                StepNumber = stepNumber,
                                Result = LibraryWorkerResultType.DataError
                            };
                        }

                        else
                        {
                            return new LibraryWorkerStepResult()
                            {
                                Completed = false,
                                Message = "Music Brainz client lookup error:  " + entity.FileName,
                                StepNumber = stepNumber,
                                Result = LibraryWorkerResultType.ServiceFailure
                            };
                        }
                    }
                }

                return LibraryWorkerStepResult.Success(stepNumber, "Music Brainz service successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error retrieving Music Brainz (basic) information: " + ex.Message);
            }
        }

        private LibraryWorkerStepResult WorkDbStep(int stepNumber)
        {
            try
            {
                var updated = 0;
                var added = 0;
                var index = 0;

                var vendorName = VendorNames.MusicBrainz.GetAttribute<DisplayAttribute>().Name;
                var vendor = _audioStationDbClient.FirstEntity<Vendor>(x => x.VendorName == vendorName);

                if (vendor == null)
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Failed to find 'Music Brainz' vendor in database. Please ensure that this vendor has been added to your configuration",
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultType.Failure
                    };
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
                        existingEntity.ImportWorkflowId = result.ImportWorkflowId;

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

                return LibraryWorkerStepResult.Success(stepNumber, string.Format("Music Brainz results imported to database:  {0} added, {1} updated", added, updated));
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error importing Music Brainz (basic) data: " + ex.Message);
            }
        }
    }
}
