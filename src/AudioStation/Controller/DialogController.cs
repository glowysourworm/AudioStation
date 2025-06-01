using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Controller.Interface;
using AudioStation.Views;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AudioStation.Controller
{
    public class DialogController : IDialogController
    {
        private MainWindow _mainWindow;

        public DialogController() 
        {

        }

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public async Task<string> ShowSaveFile(FilePickerSaveOptions options)
        {
            var file = await _mainWindow.StorageProvider.SaveFilePickerAsync(options);

            if (file != null)
            {
                return file.Path.LocalPath;
            }

            return "";
        }

        public async Task<string> ShowSelectFile()
        {
            throw new NotImplementedException();
        }

        public async Task<string> ShowSelectFolder(FolderPickerOpenOptions options)
        {
            // Start async operation to open the dialog.
            var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Library Directory (Base)",
                AllowMultiple = false
            });

            if (folders.Count >= 1)
            {
                return folders[0].Path.LocalPath;
            }

            return "";
        }
    }
}
