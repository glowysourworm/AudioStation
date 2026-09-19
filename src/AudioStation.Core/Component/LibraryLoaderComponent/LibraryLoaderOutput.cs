using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderOutput<T> : ILibraryLoaderOutput
    {
        List<LogMessage> _log;
        List<LibraryWorkerStepResult> _resultSteps;
        int _numberOfSteps;
        T _payload;

        public T Payload { get { return _payload; } }
        public IEnumerable<LibraryWorkerStepResult> ResultSteps { get { return _resultSteps; } }
        public IEnumerable<LogMessage> CurrentLog { get { return _log; } }
        public LibraryLoadType LoadType { get; private set; }
        object ILibraryLoaderOutput.Payload { get { return _payload; } }

        public LibraryLoaderOutput(LibraryLoadType loadType, T output, int numberOfSteps)
        {
            _payload = output;
            this.LoadType = loadType;

            _log = new List<LogMessage>();
            _resultSteps = new List<LibraryWorkerStepResult>();
            _numberOfSteps = numberOfSteps;
        }

        public void SetPayload(T newPayload)
        {
            _payload = newPayload;
        }

        public void AddResultStep(LibraryWorkerStepResult result)
        {
            if (_resultSteps.Count >= _numberOfSteps)
                throw new ArgumentException("Result step count exceeds the number of worker steps");

            // Log
            _log.Add(new LogMessage(result.Message));
            _resultSteps.Add(result);
        }

        public int GetNumberOfSteps()
        {
            return _numberOfSteps;
        }

        public void Log(string message)
        {
            _log.Add(new LogMessage(message));
        }

        public IEnumerable<LogMessage> GetLog()
        {
            return _log;
        }

        public IEnumerable<LibraryWorkerStepResult> GetResults()
        {
            return _resultSteps;
        }
    }
}
