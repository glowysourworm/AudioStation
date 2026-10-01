using SimpleWpf.UI.ViewModel;

namespace AudioStation.Event.DialogEvents.DialogEditorEvents
{
    public abstract class DialogEditorViewModelBase : ViewModelBase
    {
        bool _isValid;
        bool _isComplete;
        string _validationMessage;

        public bool IsValid
        {
            get { return _isValid; }
            private set { this.RaiseAndSetIfChanged(ref _isValid, value); }
        }
        public bool IsComplete
        {
            get { return _isComplete; }
            private set { this.RaiseAndSetIfChanged(ref _isComplete, value); }
        }
        public string ValidationMessage
        {
            get { return _validationMessage; }
            private set { this.RaiseAndSetIfChanged(ref _validationMessage, value); }
        }

        protected abstract bool Validate(out bool isComplete, out string validationMessage);

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            if (name != nameof(IsValid) &&
                name != nameof(IsComplete) &&
                name != nameof(ValidationMessage))
            {
                _isValid = Validate(out _isComplete, out _validationMessage);

                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(IsComplete));
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }

        public DialogEditorViewModelBase()
        {
            _validationMessage = string.Empty;
            _isValid = false;
            _isComplete = false;
        }
    }
}