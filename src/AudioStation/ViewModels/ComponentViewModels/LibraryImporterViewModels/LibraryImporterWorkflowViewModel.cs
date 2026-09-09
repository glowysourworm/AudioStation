using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterWorkflowViewModel : ViewModelBase
    {
        string _directory;
        DateTime _createdDate;
        DateTime _modifiedDate;
        int _stepNumber;

        int _lastUIStepNumber;
        bool _improperShutdown;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
        }
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { this.RaiseAndSetIfChanged(ref _createdDate, value); }
        }
        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
            set { this.RaiseAndSetIfChanged(ref _modifiedDate, value); }
        }
        public int StepNumber
        {
            get { return _stepNumber; }
            set { this.RaiseAndSetIfChanged(ref _stepNumber, value); }
        }

        public int LastUIStepNumber
        {
            get { return _lastUIStepNumber; }
            set { this.RaiseAndSetIfChanged(ref _lastUIStepNumber, value); }
        }
        public bool ImproperShutdown
        {
            get { return _improperShutdown; }
            set { this.RaiseAndSetIfChanged(ref _improperShutdown, value); }
        }


        public LibraryImporterWorkflowViewModel()
        {
            this.Directory = string.Empty;
        }
    }
}
