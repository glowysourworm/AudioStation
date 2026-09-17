using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderWorkItemUpdate
    {
        public int Id { get; private set; }
        public int OwnerId { get; private set; }
        public LibraryLoadType Type { get; private set; }
        public IEnumerable<LibraryWorkerStepResult> ResultStepsCompleted { get; set; }
        public IEnumerable<LogMessage> Log { get; set; }
        public int ResultStepCount { get; set; }
        public LibraryWorkItemState State { get; set; }

        public LibraryLoaderWorkItemUpdate(int id,
                                           int ownerId,
                                           LibraryLoadType type,
                                           IEnumerable<LibraryWorkerStepResult> resultSteps,
                                           int numberOfSteps,
                                           IEnumerable<LogMessage> log,
                                           LibraryWorkItemState state)
        {
            this.Id = id;
            this.OwnerId = ownerId;
            this.Type = type;
            this.ResultStepsCompleted = resultSteps;
            this.ResultStepCount = numberOfSteps;
            this.Log = log;
            this.State = state;
        }
    }
}
