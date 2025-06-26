using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        int _id;
        bool _hasErrors;
        double _progress;
        string _lastMessage;
        string _logIdentity;
        TimeSpan _runtime;
        LibraryLoadType _loadType;
        LibraryWorkItemState _loadState;

        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public LibraryWorkItemState LoadState
        {
            get { return _loadState; }
            set { this.RaiseAndSetIfChanged(ref _loadState, value); }
        }
        public string LogIdentity
        {
            get { return _logIdentity; }
            set { this.RaiseAndSetIfChanged(ref _logIdentity, value); }
        }
        public string LastMessage
        {
            get { return _lastMessage; }
            set { this.RaiseAndSetIfChanged(ref _lastMessage, value); }
        }
        public TimeSpan Runtime
        {
            get { return _runtime; }
            set { this.RaiseAndSetIfChanged(ref _runtime, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
        public bool HasErrors
        {
            get { return _hasErrors; }
            set { this.RaiseAndSetIfChanged(ref _hasErrors, value); }
        }
        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); SetLogIdentity(); }
        }

        private void SetLogIdentity()
        {
            this.LogIdentity = "Log Output: (see Library Loader #" + this.Id.ToString() + ")";
        }

        public LibraryWorkItemViewModel()
        {
        }
    }
}
