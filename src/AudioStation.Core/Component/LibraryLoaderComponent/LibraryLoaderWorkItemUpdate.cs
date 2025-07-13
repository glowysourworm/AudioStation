using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderWorkItemUpdate
    {
        public int Id { get; set; }
        public LibraryLoadType Type { get; set; }
        public IEnumerable<LibraryLoaderResultStep> ResultSteps { get; set; }
        public IEnumerable<LogMessage> Log { get; set; }
        public bool IsCompleted { get; set; }

        public LibraryLoaderWorkItemUpdate(int id, LibraryLoadType type,
                                           IEnumerable<LibraryLoaderResultStep> resultSteps,
                                           IEnumerable<LogMessage> log,
                                           bool isCompleted)
        {
            this.Id = id;
            this.Type = type;
            this.ResultSteps = resultSteps;
            this.Log = log;
            this.IsCompleted = isCompleted;
        }
    }
}
