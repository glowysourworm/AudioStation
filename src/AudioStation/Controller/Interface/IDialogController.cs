namespace AudioStation.Controller.Interface
{
    public interface IDialogController : IDisposable
    {
        string ShowSelectFile();
        string ShowSelectFolder();
        string ShowSaveFile();
        bool ShowConfirmation(string caption, params string[] messageLines);
    }
}
