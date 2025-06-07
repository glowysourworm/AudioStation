using AudioStation.Controller.Interface;

using Microsoft.Win32;

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

        public void Dispose()
        {
            // TODO
        }
    }
}
