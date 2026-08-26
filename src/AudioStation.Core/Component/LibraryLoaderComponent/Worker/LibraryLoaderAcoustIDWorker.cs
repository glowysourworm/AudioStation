using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderAcoustIDWorker : LibraryLoaderWorker
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private readonly int ACOUSTID_MIN_SCORE = 70;
        private static readonly int WORK_STEPS = 2;

        public LibraryLoaderAcoustIDWorker(IAcoustIDClient acoustIDClient, IAudioStationDbClient audioStationDbClient, LibraryLoaderWorkItem workItem) : base(workItem)
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

        protected override bool Work(int step, ref string message)
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
                    return WorkAcoustIDStep(ref message);
                }
                case 2:
                {
                    return WorkDbStep(ref message);
                }
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkAcoustIDStep(ref string message)
        {
            try
            {
                var resultSet = _acoustIDClient.IdentifyFingerprint(this.Load.Get<LibraryLoaderFileLoad>().File, ACOUSTID_MIN_SCORE);

                foreach (var result in resultSet)
                {
                    this.Output.Get<LibraryLoaderEntitySetOutput<AcoustIDLookupResult>>().Add(result);
                }

                message = "AcoustID fingerprint service call successful";

                return true;
            }
            catch (Exception ex)
            {
                message = "AcoustID fingerprint service error: " + ex.Message;
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

                foreach (var result in this.Output.Get<LibraryLoaderEntitySetOutput<AcoustIDLookupResult>>().Entities)
                {
                    var existingEntity = _audioStationDbClient.FirstEntity<AcoustIDLookupResult>(x => x.MusicBrainzRecordingId == result.MusicBrainzRecordingId);

                    // Update
                    if (existingEntity != null)
                    {
                        existingEntity.FileName = this.Load.Get<LibraryLoaderFileLoad>().File;
                        existingEntity.Fingerprint = result.Fingerprint;
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

                message = string.Format("AcoustID results imported to database:  {0} added, {1} updated", added, updated);

                return true;
            }
            catch (Exception ex)
            {
                message = "AcoustID database import error " + ex.Message;
                return false;
            }
        }
    }
}
