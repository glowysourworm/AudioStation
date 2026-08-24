using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Vendor.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderAcoustIDWorker : LibraryWorkerThreadBase
    {
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IAudioStationDbClient _audioStationDbClient;

        private readonly int ACOUSTID_MIN_SCORE = 70;
        private readonly int WORK_STEPS = 2;

        private LibraryLoaderFileLoad _workLoad;
        private LibraryLoaderEntitySetOutput<AcoustIDLookupResult> _workOutput;

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderAcoustIDWorker(IAcoustIDClient acoustIDClient, IAudioStationDbClient audioStationDbClient, LibraryLoaderWorkItem workItem) : base(workItem)
        {
            _acoustIDClient = acoustIDClient;
            _audioStationDbClient = audioStationDbClient;

            _workLoad = workItem.GetWorkItem() as LibraryLoaderFileLoad;
            _workOutput = workItem.GetOutputItem() as LibraryLoaderEntitySetOutput<AcoustIDLookupResult>;
        }

        public override int GetNumberOfWorkSteps()
        {
            return WORK_STEPS;
        }

        public override int GetCurrentWorkStep()
        {
            lock (_lock)
            {
                return _workCurrentStep;
            }
        }

        protected override bool WorkNext()
        {
            // Steps:
            //
            // 1) AcoustID
            // 2) Database Import AcoustID Entit(y|ies)
            // 

            IncrementWorkStep();

            switch (_workCurrentStep)
            {
                // Import:  Assume no tag data is filled out. Go with the best acoustID result you can
                //          get; and hope that it works right out of the box.
                //
                case 1:
                {
                    var message = string.Empty;
                    var success = WorkAcoustIDStep(ref message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 2:
                {
                    var message = string.Empty;
                    var success = WorkDbStep(ref message);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                default:
                    throw new Exception("Unhandled work step");
            }
        }

        private bool WorkAcoustIDStep(ref string message)
        {
            try
            {
                _workOutput.ResultSet = _acoustIDClient.IdentifyFingerprint(_workLoad.File, ACOUSTID_MIN_SCORE);

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

                foreach (var result in _workOutput.ResultSet)
                {
                    var existingEntity = _audioStationDbClient.FirstEntity<AcoustIDLookupResult>(x => x.MusicBrainzRecordingId == result.MusicBrainzRecordingId);

                    // Update
                    if (existingEntity != null)
                    {
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

        private void IncrementWorkStep()
        {
            lock (_lock)
            {
                _workCurrentStep++;
            }
        }
    }
}
