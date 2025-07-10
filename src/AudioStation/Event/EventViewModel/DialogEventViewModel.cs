using SimpleWpf.Extensions;

namespace AudioStation.Event.EventViewModel
{
    public class DialogEventViewModel : ViewModelBase
    {
        bool _userDismissalMode;
        string _dialogTitle;
        DialogViewModelBase _viewDataContext;

        public bool UserDismissalMode
        {
            get { return _userDismissalMode; }
            set { this.RaiseAndSetIfChanged(ref _userDismissalMode, value); }
        }
        public string DialogTitle
        {
            get { return _dialogTitle; }
            set { this.RaiseAndSetIfChanged(ref _dialogTitle, value); }
        }
        public DialogViewModelBase ViewDataContext
        {
            get { return _viewDataContext; }
            set { this.RaiseAndSetIfChanged(ref _viewDataContext, value); }
        }

        public DialogEventViewModel()
        {
            this.UserDismissalMode = false;
            this.DialogTitle = string.Empty;
        }
    }
}
