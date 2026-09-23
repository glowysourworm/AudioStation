using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service;
using AudioStation.Core.Service.Payload.Input;
using AudioStation.Core.Service.Payload.Output;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;

using IF.Lastfm.Core.Api.Helpers;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderMusicBrainzBasicWorker : LibraryLoaderWorker<LibraryLoaderMusicBrainzBasicPayload, LibraryLoaderMusicBrainzBasicOutputPayload>
    {
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private const int WORK_STEPS = 3;

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
                    return WorkAcoustIDResultsStep(step);
                case 2:
                    return WorkMusicBrainzTagOptionalStep(step);
                case 3:
                    return WorkDbStep(step);
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkAcoustIDResultsStep(int stepNumber)
        {
            try
            {
                var hasInvalidTags = false;

                foreach (var entity in this.Load.Payload.AcoustIDResults)
                {
                    // Valid Entities Only (from AcoustID lookup)
                    if (entity.MusicBrainzRecordingId != Guid.Empty)
                    {

                        Log("Music Brainz client lookup started:  " + entity.FileName);

                        var requestPayload = new MusicBrainzLookupPayload(MusicBrainzLookupRequestType.MusicBrainzRecordingId, entity.MusicBrainzRecordingId);
                        var request = new AudioStationTagServiceRequest(AudioStationTagRequestType.TagSmall, requestPayload);
                        var response = _musicBrainzClient.ProcessRequest(request);

                        if (response.Success)
                        {
                            var result = (response.Payload as TagSmallPayload).Data;
                            var validation = TagValidator.ValidateTag(result);

                            if (validation.IsValid)
                            {
                                Log("Music Brainz client lookup finished (valid):  " + entity.FileName);
                            }
                            else
                            {
                                Log("Music Brainz client lookup finished (invalid):  " + entity.FileName);
                                Log("VALIDATION:  " + validation.ValidationMessage);

                                hasInvalidTags = true;
                            }

                            // Keep (valid / invalid) Tag
                            var tagSmall = _audioStationMapper.Map<ITagSmall, TagSmall>(result);

                            // This may be used in the workflow; but will not be
                            // backed up in the database
                            this.Output.Payload.AcoustIDResults.Add(tagSmall);
                        }

                        else
                        {
                            return new LibraryWorkerStepResult()
                            {
                                Completed = false,
                                Message = "Music Brainz client lookup error:  " + entity.FileName,
                                StepNumber = stepNumber,
                                Result = LibraryWorkerResultLevel.ServiceFailure
                            };
                        }
                    }
                }

                return new LibraryWorkerStepResult()
                {
                    Completed = true,
                    Message = !hasInvalidTags ? "Music Brainz (basic) finished AcoustID result lookup" :
                                                "Music Brainz (basic) finished AcoustID result lookup with some invalid tag data",
                    StepNumber = stepNumber,
                    Result = !hasInvalidTags ? LibraryWorkerResultLevel.Success : LibraryWorkerResultLevel.DataWarning
                };
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error retrieving Music Brainz (basic) information: " + ex.Message);
            }
        }

        private LibraryWorkerStepResult WorkMusicBrainzTagOptionalStep(int stepNumber)
        {
            // Check Configuration Option
            if (!this.Load.Payload.PerformExtraTagLookup)
            {
                return LibraryWorkerStepResult.Success(stepNumber, "Music Brainz tag lookup option not selected, continuing with next step");
            }
            else if (this.Load.Payload.MusicBrainzReleaseTrackIDTag == null)
            {
                return new LibraryWorkerStepResult()
                {
                    Completed = true,
                    Message = "Music Brainz tag lookup data not found in IDV32 tag",
                    Result = LibraryWorkerResultLevel.DataError,
                    StepNumber = stepNumber
                };
            }

            try
            {

                Log("Music Brainz client lookup started:  " + this.Load.Payload.FileName);

                var requestPayload = new MusicBrainzLookupPayload(MusicBrainzLookupRequestType.MusicBrainzReleaseTrackId, this.Load.Payload.MusicBrainzReleaseTrackIDTag);
                var request = new AudioStationTagServiceRequest(AudioStationTagRequestType.TagSmall, requestPayload);
                var response = _musicBrainzClient.ProcessRequest(request);
                var hasInvalidTags = false;

                if (response.Success)
                {
                    var result = (response.Payload as TagSmallPayload).Data;
                    var validation = TagValidator.ValidateTag(result);

                    if (validation.IsValid)
                    {
                        Log("Music Brainz client lookup finished (valid):  " + this.Load.Payload.FileName);
                    }
                    else
                    {
                        Log("Music Brainz client lookup finished (invalid):  " + this.Load.Payload.FileName);
                        Log("VALIDATION:  " + validation.ValidationMessage);

                        hasInvalidTags = true;
                    }

                    // Keep (valid / invalid) Tag
                    var tagSmall = _audioStationMapper.Map<ITagSmall, TagSmall>(result);

                    // This may be used in the workflow; but will not be
                    // backed up in the database
                    this.Output.Payload.MusicBrainzResult = tagSmall;
                }

                else
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = false,
                        Message = "Music Brainz client lookup error:  " + this.Load.Payload.FileName,
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultLevel.ServiceFailure
                    };
                }

                return new LibraryWorkerStepResult()
                {
                    Completed = true,
                    Message = !hasInvalidTags ? "Music Brainz (basic) finished AcoustID result lookup" :
                                                "Music Brainz (basic) finished AcoustID result lookup with some invalid tag data",
                    StepNumber = stepNumber,
                    Result = !hasInvalidTags ? LibraryWorkerResultLevel.Success : LibraryWorkerResultLevel.DataWarning
                };
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
                        Result = LibraryWorkerResultLevel.Failure
                    };
                }

                // AcoustID -> Music Brainz Results
                foreach (var result in this.Output.Payload.AcoustIDResults)
                {
                    Log("Importing Music Brainz result to database:  " + result.Title);

                    var inputLoad = this.Load.Payload.AcoustIDResults.ElementAt(index++);
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

                return LibraryWorkerStepResult.Success(stepNumber, string.Format("Music Brainz results imported to database:  {0} added, {1} updated", added, updated));
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "Error importing Music Brainz (basic) data: " + ex.Message);
            }
        }
    }
}
