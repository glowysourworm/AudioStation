namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public abstract class LibraryLoaderWorker : LibraryWorkerThreadBase
    {
        protected LibraryLoaderLoad Load { get; private set; }
        protected LibraryLoaderOutput Output { get; private set; }
        protected int? WorkflowId { get; private set; }

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderWorker(LibraryLoaderWorkItem workItem) : base(workItem)
        {
            this.Load = workItem.GetWorkItem();
            this.Output = workItem.GetOutputItem();
            this.WorkflowId = workItem.GetIsWorkflowTask() ? workItem.GetWorkflowId() : null;
        }

        /// <summary>
        /// Returns number of work steps to the caller
        /// </summary>
        /// <returns></returns>
        public override abstract int GetNumberOfWorkSteps();

        public sealed override int GetCurrentWorkStep()
        {
            lock (_lock)
            {
                return _workCurrentStep;
            }
        }

        protected sealed override bool WorkNext()
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

            var result = Work(_workCurrentStep);
            this.Output.AddResultStep(result);

            switch (result.Result)
            {
                case LibraryWorkerResultType.Success:
                    return result.Completed;

                case LibraryWorkerResultType.Failure:
                    return false;

                case LibraryWorkerResultType.DataError:
                    return result.Completed;

                case LibraryWorkerResultType.ServiceNoResult:
                    return result.Completed;

                case LibraryWorkerResultType.ServiceFailure:
                    return result.Completed;
                default:
                    throw new Exception("Unhandled worker result type");
            }
        }

        protected void Log(string message)
        {
            this.Output.Log(message);
        }
        protected void Log(string message, params object[] formatParameters)
        {
            this.Output.Log(string.Format(message, formatParameters));
        }

        protected abstract LibraryWorkerStepResult Work(int stepNumber);

        private void IncrementWorkStep()
        {
            lock (_lock)
            {
                _workCurrentStep++;
            }
        }
    }
}
