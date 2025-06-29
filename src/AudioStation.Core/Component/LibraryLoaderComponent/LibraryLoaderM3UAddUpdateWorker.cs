using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.M3U;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderM3UAddUpdateWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;

        public LibraryLoaderM3UAddUpdateWorker(IModelController modelController)
        {
            _modelController = modelController;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            // Procedure:  Progress updates are polled from the LibraryLoader when
            //             state change events are fired.
            //
            // 1) Loop through items on this thread
            // 2) Do one at a time
            // 

            var fileLoad = workItem.GetWorkItem() as LibraryLoaderFileLoad;
            var generalError = false;

            // Processing...
            workItem.Start();

            foreach (var file in fileLoad.GetPendingFiles())
            {
                var streams = LoadRadioEntry(workItem.GetId(), file);

                // Set Work Item
                if (streams == null || streams.Count == 0)
                {
                    ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load failed:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, file);

                    generalError = true;
                }
                else
                {
                    // Add to database
                    _modelController.AddRadioEntries(streams);

                    ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load success: Streams={0}, File={1}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Information, streams.Count, file);
                }

                fileLoad.SetComplete(file, streams != null && streams.Count > 0);

                // Report -> UI Dispatcher (progress update)
                //
                Report(new LibraryWorkItem()
                {
                    Id = workItem.GetId(),
                    HasErrors = workItem.GetHasErrors(),
                    LoadState = workItem.GetLoadState(),
                    LoadType = workItem.GetLoadType(),
                    FailureCount = workItem.GetFailureCount(),
                    SuccessCount = workItem.GetSuccessCount(),
                    EstimatedCompletionTime = DateTime.Now.AddSeconds(DateTime.Now.Subtract(workItem.GetStartTime()).TotalSeconds / (workItem.GetPercentComplete() == 0 ? 1 : workItem.GetPercentComplete())),
                    PercentComplete = workItem.GetPercentComplete(),
                    LastMessage = file
                });
            }

            if (generalError)
                workItem.Update(LibraryWorkItemState.CompleteError);

            else
                workItem.Update(LibraryWorkItemState.CompleteSuccessful);
        }

        public List<M3UStream> LoadRadioEntry(int workItemId, string fileName)
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
                ApplicationHelpers.LogSeparate(workItemId, "Radio M3U file load error:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, ex.Message);
            }

            return null;
        }
    }
}
