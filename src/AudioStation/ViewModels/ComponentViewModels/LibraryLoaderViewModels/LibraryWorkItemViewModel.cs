using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        int _id;
        LibraryLoaderLoadViewModelBase _load;
        LibraryLoaderOutputViewModelBase _output;
        LibraryLoadType _loadType;
        ObservableCollection<LibraryLoaderWorkStepViewModel> _workSteps;
        ObservableCollection<LogMessageViewModel> _logMessages;
        int _progress;
        bool _inProgress;
        bool _isCompleted;
        bool _hasErrors;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { this.RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public ObservableCollection<LibraryLoaderWorkStepViewModel> WorkSteps
        {
            get { return _workSteps; }
            set { this.RaiseAndSetIfChanged(ref _workSteps, value); }
        }
        public ObservableCollection<LogMessageViewModel> LogMessages
        {
            get { return _logMessages; }
            set { this.RaiseAndSetIfChanged(ref _logMessages, value); }
        }
        public LibraryLoaderLoadViewModelBase Load
        {
            get { return _load; }
            set { this.RaiseAndSetIfChanged(ref _load, value); }
        }
        public LibraryLoaderOutputViewModelBase Output
        {
            get { return _output; }
            set { this.RaiseAndSetIfChanged(ref _output, value); }
        }
        public int Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
        public bool InProgress
        {
            get { return _inProgress; }
            set { this.RaiseAndSetIfChanged(ref _inProgress, value); }
        }
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set { this.RaiseAndSetIfChanged(ref _isCompleted, value); }
        }
        public bool HasErrors
        {
            get { return _hasErrors; }
            set { this.RaiseAndSetIfChanged(ref _hasErrors, value); }
        }

        public LibraryWorkItemViewModel()
        {
            this.WorkSteps = new ObservableCollection<LibraryLoaderWorkStepViewModel>();
            this.LogMessages = new ObservableCollection<LogMessageViewModel>();
        }
    }
}
