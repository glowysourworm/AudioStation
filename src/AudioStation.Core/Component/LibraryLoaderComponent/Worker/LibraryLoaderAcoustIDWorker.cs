using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderAcoustIDWorker : LibraryLoaderWorker<LibraryLoaderFilePayload, IList<AcoustIDLookupResult>>
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private static readonly int WORK_STEPS = 2;

        public LibraryLoaderAcoustIDWorker(IAcoustIDClient acoustIDClient, IAudioStationDbClient audioStationDbClient, LibraryLoaderWorkItem workItem)
            : base(workItem)
        {
            _acoustIDClient = acoustIDClient;
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
            // Steps:
            //
            // 1) AcoustID
            // 2) Database Import AcoustID Entit(y|ies)
            // 

            switch (step)
            {
                case 1:
                {
                    return WorkAcoustIDStep(step);
                }
                case 2:
                {
                    return WorkDbStep(step);
                }
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private LibraryWorkerStepResult WorkAcoustIDStep(int stepNumber)
        {
            try
            {
                var resultSet = _acoustIDClient.IdentifyFingerprint(this.Load.Payload.File);

                foreach (var result in resultSet)
                {
                    this.Output.Payload.Add(result);
                }

                if (!resultSet.Any())
                {
                    return new LibraryWorkerStepResult()
                    {
                        Completed = true,
                        Message = "AcoustID fingerprint service did not find any match",
                        StepNumber = stepNumber,
                        Result = LibraryWorkerResultLevel.ServiceNoResult
                    };
                }

                return LibraryWorkerStepResult.Success(stepNumber, "AcoustID fingerprint service call successful");
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "AcoustID fingerprint service error: " + ex.Message);
            }
        }

        private LibraryWorkerStepResult WorkDbStep(int stepNumber)
        {
            try
            {
                var updated = 0;
                var added = 0;

                foreach (var result in this.Output.Payload)
                {
                    var existingEntity = _audioStationDbClient.FirstEntity<AcoustIDLookupResult>(x => x.MusicBrainzRecordingId == result.MusicBrainzRecordingId);

                    // Update
                    if (existingEntity != null)
                    {
                        existingEntity.FileName = this.Load.Payload.File;
                        existingEntity.LookupId = result.LookupId;
                        existingEntity.MusicBrainzRecordingId = result.MusicBrainzRecordingId;
                        existingEntity.Score = result.Score;

                        _audioStationDbClient.UpdateEntity(existingEntity);

                        updated++;
                    }

                    // Add
                    else
                    {
                        _audioStationDbClient.AddEntity(result);

                        added++;
                    }
                }

                return LibraryWorkerStepResult.Success(stepNumber, string.Format("AcoustID results imported to database:  {0} added, {1} updated", added, updated));
            }
            catch (Exception ex)
            {
                return LibraryWorkerStepResult.Failure(stepNumber, "AcoustID database import error " + ex.Message);
            }
        }
    }
}
