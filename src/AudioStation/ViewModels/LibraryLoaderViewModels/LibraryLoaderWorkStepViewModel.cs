using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderWorkStepViewModel : ViewModelBase
    {
        int _stepNumber;
        string _message;
        bool _success;
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
        public bool Success
        {
            get { return _success; }
            set { this.RaiseAndSetIfChanged(ref _success, value); }
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
