﻿using System.Windows;

using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
using AudioStation.ViewModels.Vendor.ATLViewModel;
using AudioStation.Views.DialogViews;
using AudioStation.Views.VendorEntryViews;
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

            eventAggregator.GetEvent<DialogEvent>().Subscribe(payload => OnLoadingChanged(payload));
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

        public void ShowConfigurationAlert()
        {
            ShowAlert("Configuration Invalid", "Please fix configuration issues in the 'Configuration' tab.");
        }

        public void ShowLogWindow(LogViewModel viewModel)
        {
            var window = new LogWindow();
            window.DataContext = viewModel;
            window.Show();
        }

        public void ShowTagWindow(TagViewModel viewModel)
        {
            var window = new TagWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        public void ShowImportOptionsWindow(LibraryLoaderImportOptionsViewModel viewModel)
        {
            var window = new ImportOptionsWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        public bool ShowDialogWindowSync(DialogEventData eventData)
        {
            if (!eventData.UserDismissalMode)
                throw new Exception("Must have user dismissal mode for synchronous dialog use");

            var ready = LoadDialogWindow(eventData);

            if (ready)
            {
                var result = _dialogWindow.ShowDialog() ?? false;

                _dialogWindow.Close();
                _dialogWindow = null;

                return result;
            }
            else
                throw new Exception("Synchronous use of dialog interrupted another dialog event. Must first dismiss the other dialog window.");
        }

        private void OnLoadingChanged(DialogEventData data)
        {
            // Create / Destroy
            var ready = LoadDialogWindow(data);

            if (ready)
            {
                _dialogWindow.Show();
            }
        }

        // Returns true if the dialog is ready to show
        private bool LoadDialogWindow(DialogEventData data)
        {
            // Dismiss
            if (!data.Show)
            {
                if (_dialogWindow != null)
                {
                    _dialogWindow.Close();
                    _dialogWindow = null;
                }

                // Finished with our task.
                return false;
            }

            // Create Dialog
            else
            {
                if (_dialogWindow == null)
                {
                    _dialogWindow = new DialogWindow();
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
            switch (data.View)
            {
                case DialogView.Loading:
                    _dialogWindow.DataContext = new LoadingView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.SplashScreenLoading:
                    _dialogWindow.DataContext = new LoadingView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.MessageList:
                    _dialogWindow.DataContext = new MessageListView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.SelectionList:
                    _dialogWindow.DataContext = new SelectionView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.SmallAudioPlayer:
                    _dialogWindow.DataContext = new SmallAudioPlayerView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.EditorView:
                {
                    switch (data.EditorView)
                    {
                        case DialogEditorView.TagView:
                            _dialogWindow.DataContext = new TagView()
                            {
                                DataContext = data.DataContext
                            };
                            break;
                        case DialogEditorView.TagFieldView:
                            _dialogWindow.DataContext = new TagFieldView()
                            {
                                DataContext = data.DataContext
                            };
                            break;
                        case DialogEditorView.None:
                        default:
                            throw new Exception("Unhandled dialog editor view type:  DialogController.cs");
                    }
                }
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
            _dialogWindow.Height = data.DialogHeight;
            _dialogWindow.Width = data.DialogWidth;

            // Can't show the loading screen as a dialog window; but the window will appear as
            // a non-closeable window.
            _dialogWindow.Owner = Application.Current.MainWindow;

            return true;
        }

        public void Dispose()
        {
            if (_dialogWindow != null)
            {
                _dialogWindow.Close();
                _dialogWindow = null;
            }
        }
    }
}
