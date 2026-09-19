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
        LibraryWorkItemState _state;

        // UI Properties
        bool _isSelected;

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
        public LibraryWorkItemState State
        {
            get { return _state; }
            set { this.RaiseAndSetIfChanged(ref _state, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name != "DisplayName")
            {
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
            this.State = LibraryWorkItemState.Pending;
        }
    }
}
