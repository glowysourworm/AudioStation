using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.M3U;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderM3UAddUpdateWorker : LibraryWorkerThreadBase
    {
        private readonly IModelController _modelController;

        public LibraryLoaderM3UAddUpdateWorker(IModelController modelController,
                                               IOutputController outputController) : base(outputController)
        {
            _modelController = modelController;
        }

        protected override void Work(ref LibraryLoaderWorkItem workItem)
        {
            var streams = LoadRadioEntry(workItem.FileName);

            // Set Work Item
            if (streams == null || streams.Count == 0)
            {
                workItem.Set(LibraryWorkItemState.CompleteError, "Error loading .m3u file:  " + workItem.FileName);
            }
            else
            {
                // Add to database
                _modelController.AddRadioEntries(streams);

                workItem.Set(LibraryWorkItemState.CompleteSuccessful, "");
            }
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

                if (validMedia.Any())
                    RaiseLog("Radio M3U file loaded:  {0}", LogMessageType.General, LogLevel.Information, fileName);

                return validMedia.ToList();
            }
            catch (Exception ex)
            {
                RaiseLog("Radio M3U file load error:  {0}", LogMessageType.General, LogLevel.Error, fileName);
            }

            return null;
        }
    }
}
