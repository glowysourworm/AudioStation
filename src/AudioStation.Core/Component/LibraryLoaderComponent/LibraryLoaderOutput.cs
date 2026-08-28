using AudioStation.Core.Component.LibraryLoaderComponent.Load;
using AudioStation.Core.Component.LibraryLoaderComponent.Output;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderOutput
    {
        object _output;
        List<LogMessage> _log;
        List<LibraryLoaderResultStep> _resultSteps;
        int _numberOfSteps;
        Type _actualType;


        public LibraryLoaderOutput(LibraryLoadType loadType, object output, int numberOfSteps)
        {
            _output = output;
            _log = new List<LogMessage>();
            _resultSteps = new List<LibraryLoaderResultStep>();
            _numberOfSteps = numberOfSteps;

            Initialize(loadType, output);
        }

        private void Initialize(LibraryLoadType loadType, object output)
        {
            if (output == null)
                throw new NullReferenceException("Library loader output not set");

            switch (loadType)
            {
                case LibraryLoadType.ImportRadio:
                    if (output is not LibraryLoaderEntityOutput<M3UStream>)
                        throw new ArgumentException("Improper library output type:  ImportRadio expects LibraryLoaderEntityOutput<M3UStream>");

                    _actualType = typeof(LibraryLoaderEntityOutput<M3UStream>);
                    break;
                case LibraryLoadType.AcoustID:
                    if (output is not LibraryLoaderEntitySetOutput<AcoustIDLookupResult>)
                        throw new ArgumentException("Improper library output type:  AcoustID expects LibraryLoaderEntitySetOutput<AcoustIDLookupResult>");

                    _actualType = typeof(LibraryLoaderEntitySetOutput<AcoustIDLookupResult>);
                    break;
                case LibraryLoadType.FileChecker:
                    if (output is not LibraryLoaderNoOutput)
                        throw new ArgumentException("Improper library output type:  FileChecker expects LibraryLoaderNoOutput");

                    _actualType = typeof(LibraryLoaderNoOutput);
                    break;
                case LibraryLoadType.MusicBrainzBasic:
                    if (output is not LibraryLoaderEntitySetOutput<TagSmall>)
                        throw new ArgumentException("Improper library output type:  MusicBrainzBasic expects LibraryLoaderEntityOutput<TagSmall>");

                    _actualType = typeof(LibraryLoaderEntitySetOutput<TagSmall>);
                    break;
                case LibraryLoadType.MusicBrainzAlbumArt:
                    if (output is not LibraryLoaderEntitySetOutput<FileReference>)
                        throw new ArgumentException("Improper library output type:  MusicBrainzAlbumArt expects LibraryLoaderEntitySetOutput<FileReference>");

                    _actualType = typeof(LibraryLoaderEntitySetOutput<FileReference>);
                    break;
                case LibraryLoadType.Import:
                    if (output is not LibraryLoaderImportOutput)
                        throw new ArgumentException("Improper library output type:  Import expects LibraryLoaderImportOutput");

                    _actualType = typeof(LibraryLoaderImportLoad);
                    break;
                default:
                    throw new Exception("Unhandled library load type");
            }

            _output = output;
        }

        /// <summary>
        /// Returns output properly casted for this output load
        /// </summary>
        public T Get<T>()
        {
            if (typeof(T) != _actualType)
                throw new ArgumentException("Imporper load type. Expecting:  " + _actualType.ToString());

            return (T)_output;
        }

        public void AddResultStep(bool result, string message)
        {
            if (_resultSteps.Count >= _numberOfSteps)
                throw new ArgumentException("Result step count exceeds the number of worker steps");

            // Log
            _log.Add(new LogMessage(message));
            _resultSteps.Add(new LibraryLoaderResultStep(false, result, _resultSteps.Count + 1, message));
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

        public IEnumerable<LibraryLoaderResultStep> GetResults()
        {
            return _resultSteps;
        }
    }
}
