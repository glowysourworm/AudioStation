namespace AudioStation.Controller.Interface
{
    public interface IDialogController : IDisposable
    {
        string ShowSelectFile();
        string ShowSelectFolder();
        string ShowSaveFile();
    }
}
