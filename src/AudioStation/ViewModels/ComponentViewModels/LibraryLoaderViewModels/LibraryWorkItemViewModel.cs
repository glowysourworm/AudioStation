using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        int _id;
        LibraryLoaderLoadViewModel _load;
        LibraryLoaderOutputViewModel _output;
        LibraryLoadType _loadType;
        ObservableCollection<LibraryLoaderWorkStepViewModel> _workSteps;
        ObservableCollection<LogMessageViewModel> _logMessages;

        // These will be about the previous run if that is implied by the database data
        double _progress;
        bool _inProgress;
        bool _isCompleted;
        bool _hasErrors;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public LibraryLoaderLoadViewModel Load
        {
            get { return _load; }
            set { this.RaiseAndSetIfChanged(ref _load, value); }
        }
        public LibraryLoaderOutputViewModel Output
        {
            get { return _output; }
            set { this.RaiseAndSetIfChanged(ref _output, value); }
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
        public double Progress
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

        public string DisplayName
        {
            get { return _load.DisplayText; }
        }

        public string Status
        {
            get
            {
                if (this.InProgress)
                    return "In Progress";

                else if (this.IsCompleted)
                    return "Completed";

                else
                    return "Queued";
            }
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name != "Status" &&
                name != "DisplayName")
            {
                OnPropertyChanged("Status");
                OnPropertyChanged("DisplayName");
            }
        }

        public LibraryWorkItemViewModel()
        {
            this.WorkSteps = new ObservableCollection<LibraryLoaderWorkStepViewModel>();
            this.LogMessages = new ObservableCollection<LogMessageViewModel>();
            this.Progress = 0;
            this.Load = new LibraryLoaderLoadViewModel();
            this.Output = new LibraryLoaderOutputViewModel();
        }

        public override string ToString()
        {
            return this.DisplayName ?? string.Empty;
        }
    }
}
