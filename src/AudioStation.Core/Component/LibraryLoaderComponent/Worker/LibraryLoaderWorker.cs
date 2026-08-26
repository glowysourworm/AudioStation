namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public abstract class LibraryLoaderWorker : LibraryWorkerThreadBase
    {
        protected LibraryLoaderLoad Load { get; private set; }
        protected LibraryLoaderOutput Output { get; private set; }

        // Thread Contention (between work steps only)
        private int _workCurrentStep = 0;
        private object _lock = new object();

        public LibraryLoaderWorker(LibraryLoaderWorkItem workItem) : base(workItem)
        {
            this.Load = workItem.GetWorkItem();
            this.Output = workItem.GetOutputItem();
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

            var message = string.Empty;
            var success = Work(_workCurrentStep, ref message);
            this.Output.AddResultStep(success, message);

            return success;
        }

        protected void Log(string message)
        {
            this.Output.Log(message);
        }

        protected abstract bool Work(int stepNumber, ref string message);

        private void IncrementWorkStep()
        {
            lock (_lock)
            {
                _workCurrentStep++;
            }
        }
    }
}
