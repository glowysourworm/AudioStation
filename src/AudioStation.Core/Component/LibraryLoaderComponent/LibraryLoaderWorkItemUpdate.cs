using AudioStation.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderWorkItemUpdate
    {
        public int Id { get; set; }
        public LibraryLoadType Type { get; set; }
        public IEnumerable<LibraryLoaderResultStep> ResultStepsCompleted { get; set; }
        public IEnumerable<LogMessage> Log { get; set; }
        public int ResultStepCount { get; set; }
        public bool IsCompleted { get; set; }

        public LibraryLoaderWorkItemUpdate(int id, LibraryLoadType type,
                                           IEnumerable<LibraryLoaderResultStep> resultSteps,
                                           int numberOfSteps,
                                           IEnumerable<LogMessage> log,
                                           bool isCompleted)
        {
            this.Id = id;
            this.Type = type;
            this.ResultStepsCompleted = resultSteps;
            this.ResultStepCount = numberOfSteps;
            this.Log = log;
            this.IsCompleted = isCompleted;
        }
    }
}
