using System.Drawing;

using SimpleWpf.Extensions;

namespace AudioStation.Event.DialogEvents
{
    public class DialogEventViewModel : ViewModelBase
    {
        bool _userDismissalMode;
        string _dialogTitle;
        Brush _dialogBackground;
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
        public Brush DialogBackground
        {
            get { return _dialogBackground; }
            set { this.RaiseAndSetIfChanged(ref _dialogBackground, value); }
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
            this.DialogBackground = Brushes.White;
        }
    }
}
