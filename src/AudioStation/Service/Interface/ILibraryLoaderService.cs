using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        void RunLoaderTaskAsync(LibraryLoaderImportLoad workLoad);
        void RunLoaderTaskAsync(LibraryLoaderEntityLoad workLoad);
    }
}
