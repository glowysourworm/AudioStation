using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        int RunLoaderTaskAsync(LibraryLoaderImportLoadViewModel workLoad);
        int RunLoaderTaskAsync(LibraryLoaderFileLoadViewModel workLoad);
    }
}
