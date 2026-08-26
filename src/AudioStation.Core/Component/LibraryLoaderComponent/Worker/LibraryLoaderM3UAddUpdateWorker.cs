using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Model.M3U;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public class LibraryLoaderM3UAddUpdateWorker : LibraryLoaderWorker
    {
        public LibraryLoaderM3UAddUpdateWorker(LibraryLoaderWorkItem workItem)
            : base(workItem)
        {
        }

        public static int GetNumberSteps()
        {
            return 1;
        }

        protected override bool Work(int step, ref string message)
        {
            var streams = LoadRadioEntry(this.Load.Get<LibraryLoaderFileLoad>().File);

            // Set Work Item
            if (streams == null || streams.Count == 0)
            {
                //ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load failed:  {0}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Error, file);
            }
            else
            {
                // Add to database
                //_modelController.AddRadioEntries(streams);

                //ApplicationHelpers.LogSeparate(workItem.GetId(), "M3U stream file load success: Streams={0}, File={1}", LogMessageType.LibraryLoaderWorkItem, LogLevel.Information, streams.Count, file);
            }

            this.Output.AddResultStep(streams != null && streams.Count > 0, "Radio Import Complete");

            return true;
        }

        public override int GetNumberOfWorkSteps()
        {
            return 1;
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
