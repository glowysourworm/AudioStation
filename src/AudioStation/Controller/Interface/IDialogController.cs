using System.Threading.Tasks;

using Avalonia.Platform.Storage;

namespace AudioStation.Controller.Interface
{
    public interface IDialogController
    {
        Task<string> ShowSelectFile();
        Task<string> ShowSelectFolder(FolderPickerOpenOptions options);
        Task<string> ShowSaveFile(FilePickerSaveOptions options);
    }
}
