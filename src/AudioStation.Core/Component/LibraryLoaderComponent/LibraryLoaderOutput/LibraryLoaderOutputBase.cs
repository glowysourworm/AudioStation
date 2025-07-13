using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderOutputBase
    {
        /// <summary>
        /// Work Item Log
        /// </summary>
        public List<LogMessage> Log { get; }

        public List<LibraryLoaderResultStep> Results { get; }

        public LibraryLoaderOutputBase()
        {
            this.Log = new List<LogMessage>();
            this.Results = new List<LibraryLoaderResultStep>();
        }

        public void SetResult(bool result, int currentStep, int totalSteps, string message)
        {
            if (currentStep > totalSteps)
                throw new ArgumentException("Improper step numbering:  LibraryLoaderOutputBase");

            while (this.Results.Count < totalSteps)
            {
                this.Results.Add(new LibraryLoaderResultStep(false, false, this.Results.Count, string.Empty));
            }

            this.Results[currentStep - 1] = new LibraryLoaderResultStep(true, result, currentStep, message);
        }
    }
}
