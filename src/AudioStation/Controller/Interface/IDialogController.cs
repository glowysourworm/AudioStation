using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.Vendor.ATLViewModel;

namespace AudioStation.Controller.Interface
{
    public interface IDialogController : IDisposable
    {
        string ShowSelectFile();
        string ShowSelectFolder();
        string ShowSaveFile();

        bool ShowConfirmation(string caption, params string[] messageLines);
        void ShowAlert(string caption, params string[] messageLines);

        void ShowLogWindow(LogViewModel viewModel);
        void ShowTagWindow(TagViewModel viewModel);

        /// <summary>
        /// Shows dialog window synchronously. This represents a parallel usage to the event aggregator! So,
        /// use this when a dialog window is needed to be waited on; and the results returned immediately.
        /// </summary>
        bool ShowDialogWindowSync(DialogEventData eventData);

        /// <summary>
        /// Shows dialog window and performs loading action. Updates progress view data.
        /// </summary>
        void ShowLoading(string title, Action<DialogEventHandlers.DialogProgressHandler> loadingAction);
    }
}
