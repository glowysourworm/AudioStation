using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        void RunLoaderTaskAsync(LibraryLoaderImportLoadViewModel workLoad);
        void RunLoaderTaskAsync(LibraryLoaderFileLoadViewModel workLoad);
    }
}
