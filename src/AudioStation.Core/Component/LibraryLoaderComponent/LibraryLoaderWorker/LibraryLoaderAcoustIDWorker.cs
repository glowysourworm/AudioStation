using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderAcoustIDWorker : LibraryLoaderWorker<LibraryLoaderFileLoad, LibraryLoaderEntitySetOutput<AcoustIDLookupResult>>
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private readonly int ACOUSTID_MIN_SCORE = 70;
        private readonly int WORK_STEPS = 2;

        public LibraryLoaderAcoustIDWorker(IAcoustIDClient acoustIDClient, IAudioStationDbClient audioStationDbClient, LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _acoustIDClient = acoustIDClient;
            _audioStationDbClient = audioStationDbClient;
        }

        public override int GetNumberOfWorkSteps()
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
                this.Output.ResultSet = _acoustIDClient.IdentifyFingerprint(this.Load.File, ACOUSTID_MIN_SCORE);

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

                foreach (var result in this.Output.ResultSet)
                {
                    var existingEntity = _audioStationDbClient.FirstEntity<AcoustIDLookupResult>(x => x.MusicBrainzRecordingId == result.MusicBrainzRecordingId);

                    // Update
                    if (existingEntity != null)
                    {
                        existingEntity.FileName = this.Load.File;
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
