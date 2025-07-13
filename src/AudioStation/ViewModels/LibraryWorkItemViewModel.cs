using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.LogViewModels;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        int _id;
        LibraryLoadType _loadType;
        ObservableCollection<LibraryLoaderWorkStepViewModel> _workSteps;
        ObservableCollection<LogMessageViewModel> _logMessages;
        bool _isCompleted;

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
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set { this.RaiseAndSetIfChanged(ref _isCompleted, value); }
        }

        public LibraryWorkItemViewModel()
        {
            this.WorkSteps = new ObservableCollection<LibraryLoaderWorkStepViewModel>();
            this.LogMessages = new ObservableCollection<LogMessageViewModel>();
        }
    }
}
