﻿using AudioStation.Event;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
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
        void ShowConfigurationAlert();

        void ShowLogWindow(LogViewModel viewModel);
        void ShowTagWindow(TagViewModel viewModel);
        void ShowImportOptionsWindow(LibraryLoaderImportOptionsViewModel viewModel);

        /// <summary>
        /// Shows dialog window synchronously. This represents a parallel usage to the event aggregator! So,
        /// use this when a dialog window is needed to be waited on; and the results returned immediately.
        /// </summary>
        bool ShowDialogWindowSync(DialogEventData eventData);
    }
}
