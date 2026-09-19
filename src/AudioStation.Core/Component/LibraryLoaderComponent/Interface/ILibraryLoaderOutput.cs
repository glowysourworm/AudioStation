using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Interface
{
    public interface ILibraryLoaderOutput
    {
        /// <summary>
        /// Returns the payload object for the output
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// Returns current result steps collection
        /// </summary>
        IEnumerable<LibraryWorkerStepResult> ResultSteps { get; }

        /// <summary>
        /// Returns the current log data for the output
        /// </summary>
        IEnumerable<LogMessage> CurrentLog { get; }

        /// <summary>
        /// Returns number of steps in the workload
        /// </summary>
        int GetNumberOfSteps();
    }
}
