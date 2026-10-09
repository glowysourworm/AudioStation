using AudioStation.Core.Model;

using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderWorkItemUpdate
    {
        public int Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public LibraryLoadType Type { get; private set; }
        public IEnumerable<LibraryWorkerStepResult> ResultStepsCompleted { get; set; }
        public IEnumerable<LogMessage> Log { get; set; }
        public int ResultStepCount { get; set; }
        public LibraryWorkItemState State { get; set; }

        public LibraryLoaderWorkItemUpdate(int id,
                                           Guid ownerId,
                                           LibraryLoadType type,
                                           IEnumerable<LibraryWorkerStepResult> resultSteps,
                                           int numberOfSteps,
                                           IEnumerable<LogMessage> log,
                                           LibraryWorkItemState state)
        {
            this.Id = id;
            this.OwnerId = ownerId;
            this.Type = type;
            this.ResultStepsCompleted = resultSteps.Select(x => new LibraryWorkerStepResult()
            {
                Completed = x.Completed,
                Message = x.Message,
                Result = x.Result,
                StepNumber = x.StepNumber

            }).Actualize();
            this.ResultStepCount = numberOfSteps;
            this.Log = log.Select(x => new LogMessage(x)).Actualize();

            this.State = state;
        }
    }
}
