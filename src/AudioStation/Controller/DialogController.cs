using System.Windows;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;
using AudioStation.Windows;

using Microsoft.Win32;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Controller
{
    [IocExport(typeof(IDialogController))]
    public class DialogController : IDialogController
    {
        [IocImportingConstructor]
        public DialogController()
        {
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

        public void ShowTagWindow(TagGroupViewModel viewModel)
        {
            var window = new TagWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        public void ShowLoadingWindow()
        {
            throw new NotImplementedException();
        }

        public void UpdateLoadingWindow(double progress)
        {
            throw new NotImplementedException();
        }

        public void HideLoadingWindow()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // TODO
        }
    }
}
