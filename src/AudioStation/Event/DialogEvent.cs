using System.Windows;
using System.Windows.Forms;

using AudioStation.Event.DialogEvents;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public enum NavigationView
    {
        None,
        Configuration,
        LibraryLoader,
        LibraryManager,
        ArtistSearch,
        NowPlaying,
        Radio,
        RadioBrowser,
        Bandcamp,
        Log
    }

    public enum DialogEventView
    {
        Loading,
        SplashScreenLoading,
        MessageList
    }

    public class DialogEventData
    {
        /// <summary>
        /// Signals that the dialog is to be displayed to the user. This does NOT imply that the
        /// dialog window will be dismissed by the user. For this, the message box buttons must
        /// be set; and only for certain dialog views.
        /// </summary>
        public bool ShowDialog { get; set; }

        /// <summary>
        /// User Dismissal Mode:  Set in the event that the dialog window is used as a send / response
        ///                       modal-interactive popup.
        /// </summary>
        public bool UserDismissalMode { get; private set; }

        /// <summary>
        /// Title for the dialog window. This will be shown centered on the banner; or the banner will be
        /// hidden.
        /// </summary>
        public string DialogTitle { get; set; }

        /// <summary>
        /// User Dismissal Mode:  Show message box buttons to the user and wait for a dialog response. This 
        ///                       should operate like a modal window. The dialog result will be completed for
        ///                       this case; and a flag will be set to show that this mode was intended.
        /// </summary>
        public MessageBoxButton MessageBoxButtons { get; set; }

        /// <summary>
        /// User Dismissal Mode:  This dialog result will be set in the event that the user has input a response
        ///                       to the dialog.
        /// </summary>
        public DialogResult DialogResult { get; set; }

        /// <summary>
        /// View for user navigation (this usually takes place after the dialog event is finished)
        /// </summary>
        public NavigationView NavigationView { get; set; }

        /// <summary>
        /// View for the content of the dialog window
        /// </summary>
        public DialogEventView EventView { get; set; }

        /// <summary>
        /// Data context for the dialog event:  (see AudioStation.Event.EventViewModel)
        /// </summary>
        public DialogViewModelBase DataContext { get; set; }

        public DialogEventData(bool showDialog = false)
            : this(showDialog, false, string.Empty, MessageBoxButton.OK, DialogResult.Abort, NavigationView.None, DialogEventView.Loading, null)
        { }

        public DialogEventData(DialogLoadingViewModel viewModel)
            : this(true, false, string.Empty, MessageBoxButton.OK, DialogResult.Abort, NavigationView.None, DialogEventView.Loading, viewModel)
        { }

        public DialogEventData(string dialogTitle, DialogMessageListViewModel viewModel)
            : this(true, true, dialogTitle, MessageBoxButton.OK, DialogResult.Abort, NavigationView.None, DialogEventView.MessageList, viewModel)
        { }

        public DialogEventData(DialogSplashScreenViewModel viewModel)
            : this(true, false, string.Empty, MessageBoxButton.OK, DialogResult.Abort, NavigationView.None, DialogEventView.SplashScreenLoading, viewModel)
        { }

        private DialogEventData(bool showDialog,
                                bool userDismissal,
                                string dialogTitle,
                                MessageBoxButton userDismissalButtons,
                                DialogResult dialogResult,
                                NavigationView requestedView,
                                DialogEventView eventView,
                                DialogViewModelBase viewDataContext)
        {
            this.UserDismissalMode = userDismissal;

            if (userDismissal)
            {
                // Make sure we're supporting the dismissal mode
                //
                switch (userDismissalButtons)
                {
                    case MessageBoxButton.OK:
                        this.MessageBoxButtons = userDismissalButtons;
                        break;
                    case MessageBoxButton.OKCancel:
                    case MessageBoxButton.YesNoCancel:
                    case MessageBoxButton.YesNo:
                    default:
                        throw new Exception("Unhandled MessageBoxButton:  DialogEvent.cs");
                }
            }

            this.ShowDialog = showDialog;
            this.DialogResult = dialogResult;
            this.NavigationView = requestedView;
            this.EventView = eventView;
            this.DataContext = viewDataContext;
            this.DialogTitle = dialogTitle;
        }

        /// <summary>
        /// Creates event data for dialog dismissal
        /// </summary>
        public static DialogEventData Dismiss(NavigationView nextView = NavigationView.None)
        {
            return new DialogEventData(false, false, string.Empty, MessageBoxButton.OK, DialogResult.Abort, nextView, DialogEventView.Loading, null);
        }

        public static DialogEventData ShowLoading(string message)
        {
            return new DialogEventData(new DialogLoadingViewModel()
            {
                Message = message
            });
        }
    }

    /// <summary>
    /// Primary loading event corresponding to the MainViewModel.Loading indicator boolean. This will
    /// be utilized to hide / show loading UI and cursor.
    /// </summary>
    public class DialogEvent : IocEvent<DialogEventData>
    {
    }
}
