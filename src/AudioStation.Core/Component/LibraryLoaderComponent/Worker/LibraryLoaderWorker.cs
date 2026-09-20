namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public abstract class LibraryLoaderWorker<TIn, TOut> : LibraryWorkerThreadBase
    {
        protected LibraryLoaderLoad<TIn> Load { get; private set; }
        protected LibraryLoaderOutput<TOut> Output { get; private set; }

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderWorker(LibraryLoaderWorkItem workItem) : base(workItem)
        {
            this.Load = workItem.GetWorkItem() as LibraryLoaderLoad<TIn>;
            this.Output = workItem.GetOutputItem() as LibraryLoaderOutput<TOut>;

            if (this.Load == null ||
                this.Output == null)
                throw new ArgumentException("Invalid Load / Output types");
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
                case LibraryWorkerResultLevel.Success:
                    return result.Completed;

                case LibraryWorkerResultLevel.Failure:
                    return false;

                case LibraryWorkerResultLevel.DataWarning:
                    return result.Completed;

                case LibraryWorkerResultLevel.DataError:
                    return result.Completed;

                case LibraryWorkerResultLevel.ServiceNoResult:
                    return result.Completed;

                case LibraryWorkerResultLevel.ServiceFailure:
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
