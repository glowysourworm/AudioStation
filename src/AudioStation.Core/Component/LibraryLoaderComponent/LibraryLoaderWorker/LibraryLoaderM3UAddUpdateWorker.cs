using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Model.M3U;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public class LibraryLoaderM3UAddUpdateWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;

        private LibraryLoaderFileLoad _workLoad;
        private LibraryLoaderOutputBase _workOutput;

        private bool _started;

        public LibraryLoaderM3UAddUpdateWorker(LibraryLoaderWorkItem workItem, IModelController modelController)
            : base(workItem)
        {
            _modelController = modelController;
            _started = false;
            _workLoad = workItem.GetWorkItem() as LibraryLoaderFileLoad;
            _workOutput = workItem.GetOutputItem() as LibraryLoaderOutputBase;
        }

        protected override bool WorkNext()
        {
            _started = true;

            var streams = LoadRadioEntry(_workLoad.File);

            // Set Work Item
            if (streams == null || streams.Count == 0)
            {
                //ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load failed:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, file);
            }
            else
            {
                // Add to database
                _modelController.AddRadioEntries(streams);

                //ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load success: Streams={0}, File={1}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Information, streams.Count, file);
            }

            _workOutput.SetResult(streams != null && streams.Count > 0, 1, 1, "Radio Import Complete");

            return true;
        }

        public override int GetNumberOfWorkSteps()
        {
            return 1;
        }

        public override int GetCurrentWorkStep()
        {
            return _started ? 1 : 0;
        }

        public List<M3UStream> LoadRadioEntry(string fileName)
        {
            List<M3UStream> m3uData = null;

            try
            {
                // Adding a nested try / catch for these files
                m3uData = M3UParser.Parse(fileName, (no, op) => { });

                // RadioEntry:  According to the M3U standard, a stream source must have a 
                //              duration setting of 0, or -1. We then should have a single
                //              media info. We can also add multiple with the same name; but
                //              there's really no reason to do this.
                //
                // Stream:      A streaming source will have at least one M3UMediaInfo entry;
                //              and this will have a duration of 0, or -1.
                //

                var validMedia = m3uData.Where(x => !string.IsNullOrEmpty(x.StreamSource) &&
                                                    !string.IsNullOrEmpty(x.Title))
                                        .DistinctBy(x => x.Title);

                return validMedia.ToList();
            }
            catch (Exception ex)
            {
                //ApplicationHelpers.LogSeparate(workItemId, "Radio M3U file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
            }

            return null;
        }
    }
}
