using AudioStation.ViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

namespace AudioStation.Controller.Interface
{
    public interface IDialogController : IDisposable
    {
        string ShowSelectFile();
        string ShowSelectFolder();
        string ShowSaveFile();
        bool ShowConfirmation(string caption, params string[] messageLines);

        void ShowLogWindow(LogViewModel viewModel);
        void ShowTagWindow(TagViewModel viewModel);
        void ShowTagWindow(TagGroupViewModel viewModel);

        void ShowLoadingWindow();
        void UpdateLoadingWindow(double progress);
        void HideLoadingWindow();
    }
}
