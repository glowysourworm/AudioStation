using System.Windows;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.ViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;
using AudioStation.Views.DialogViews;
using AudioStation.Windows;

using Microsoft.Win32;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IDialogController))]
    public class DialogController : IDialogController
    {
        private readonly IIocEventAggregator _eventAggregator;

        private DialogWindow _dialogWindow;

        [IocImportingConstructor]
        public DialogController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dialogWindow = null;

            eventAggregator.GetEvent<DialogEvent>().Subscribe(OnLoadingChanged);
        }

        public void Initialize()
        {

        }

        public string ShowSaveFile()
        {
            var dialog = new SaveFileDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        public string ShowSelectFile()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        public string ShowSelectFolder()
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                return dialog.FolderName;
            }

            return string.Empty;
        }

        public bool ShowConfirmation(string caption, params string[] messageLines)
        {
            var message = messageLines.Join("\n", x => x);

            return MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes;
        }

        public void ShowAlert(string caption, params string[] messageLines)
        {
            var message = messageLines.Join("\n", x => x);

            MessageBox.Show(message, caption, MessageBoxButton.OK);
        }

        public void ShowLogWindow(LogViewModel viewModel)
        {
            var window = new LogWindow();
            window.DataContext = viewModel;
            window.Show();
        }

        public void ShowTagWindow(TagFileGroupViewModel viewModel)
        {
            var window = new TagWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        private void OnLoadingChanged(DialogEventData data)
        {
            if (!data.ShowDialog)
            {
                if (_dialogWindow != null)
                {
                    _dialogWindow.Close();
                    _dialogWindow = null;
                }

                // Finished with our task.
                return;
            }
            else
            {
                if (_dialogWindow == null)
                {
                    _dialogWindow = new DialogWindow();
                    _dialogWindow.DialogResultEvent += OnDialogResult;
                }

                else
                    throw new Exception("Unhandled closing of current dialog. Must send dialog finished event (IsLoading = false)");
            }

            // Window.DataContext Binding:  We're using the data context to add the content presenter's data.
            //                              This is because there is no control template for the window's content.
            //                              Apparently, this is a common pattern for custom dialogs in WPF.
            //
            //                              The inner data context is for the actual data for the view. This binding
            //                              should behave as normal.
            //
            switch (data.EventView)
            {
                case DialogEventView.Loading:
                    _dialogWindow.DataContext = new LoadingView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogEventView.SplashScreenLoading:
                    _dialogWindow.DataContext = new LoadingView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogEventView.MessageList:
                    _dialogWindow.DataContext = new MessageListView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                default:
                    throw new Exception("Unhandled dialog view type:  DialogController.cs");
            }

            // We can't add this to the binding data because it is for the window. The data context is now being used
            // for the actual view content. So, we should try to keep this pattern so this dialog controller owns the
            // DialogWindow.
            //
            _dialogWindow.TitleTB.Text = data.DialogTitle;
            _dialogWindow.HeaderContainer.Visibility = string.IsNullOrEmpty(data.DialogTitle) ? Visibility.Collapsed : Visibility.Visible;
            _dialogWindow.ButtonPanel.Visibility = data.UserDismissalMode ? Visibility.Visible : Visibility.Collapsed;

            // Can't show the loading screen as a dialog window; but the window will appear as
            // a non-closeable window.
            _dialogWindow.Owner = Application.Current.MainWindow;
            _dialogWindow.Show();
        }

        private void OnDialogResult(System.Windows.Forms.DialogResult sender)
        {
            // Design:  This could be done more cleanly; but lets go ahead and use our pattern
            //          with the dialog event. Set the show dialog to false; and re-call our above 
            //          code. The rest of the application will behave as if there was another sender
            //          for a dialog response event.
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
