using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderImportWorker : LibraryWorkerThreadBase
    {
        private readonly ILibraryImporter _libraryImporter;

        private const int WORK_STEPS = 6;

        private LibraryLoaderImportLoad _workLoad;
        private LibraryLoaderImportOutput _workOutput;

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderImportWorker(LibraryLoaderWorkItem workItem,
                                         ILibraryImporter libraryImporter)
            : base(workItem)
        {
            _workLoad = workItem.GetWorkItem() as LibraryLoaderImportLoad;
            _workOutput = workItem.GetOutputItem() as LibraryLoaderImportOutput;

            _libraryImporter = libraryImporter;
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
            // 2) Music Brainz
            // 3) Embed Tag File
            // 4) Import Entity
            // 5) Migrate File (optional)
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
                    var success = _libraryImporter.WorkAcoustID(_workLoad, _workOutput).Result;
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 2:
                {
                    var message = string.Empty;
                    var success = _libraryImporter.WorkMusicBrainzDetail(_workLoad, _workOutput).Result;
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 3:
                {
                    var message = string.Empty;
                    var success = _libraryImporter.WorkMusicBrainzCompleteRecord(_workLoad, _workOutput).Result;
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 4:
                {
                    var message = string.Empty;
                    var success = _libraryImporter.WorkEmbedTag(_workLoad, _workOutput);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 5:
                {
                    var message = string.Empty;
                    var success = _libraryImporter.WorkImportEntity(_workLoad, _workOutput);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                case 6:
                {
                    var message = string.Empty;
                    var success = _libraryImporter.WorkMigrateFile(_workLoad, _workOutput);
                    _workOutput.SetResult(success, _workCurrentStep, WORK_STEPS, message);
                    return success;
                }
                default:
                    throw new Exception("Unhandled LibraryLoaderImportWorker.cs step");
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
