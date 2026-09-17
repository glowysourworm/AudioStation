using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderWorkStepViewModel : ViewModelBase
    {
        int _stepNumber;
        string _message;
        LibraryWorkerResultLevel _result;
        bool _complete;

        public int StepNumber
        {
            get { return _stepNumber; }
            set { this.RaiseAndSetIfChanged(ref _stepNumber, value); }
        }
        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public LibraryWorkerResultLevel Result
        {
            get { return _result; }
            set { this.RaiseAndSetIfChanged(ref _result, value); }
        }
        public bool Complete
        {
            get { return _complete; }
            set { this.RaiseAndSetIfChanged(ref _complete, value); }
        }

        public LibraryLoaderWorkStepViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
