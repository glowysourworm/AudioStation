using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        void RunLoaderTaskAsync(LibraryLoaderImportBasicLoad workLoad);
        void RunLoaderTaskAsync(LibraryLoaderImportDetailLoad workLoad);
        void RunLoaderTaskAsync(LibraryLoaderEntityLoad workLoad);
    }
}
