using AudioStation.ViewModels.LibraryLoaderViewModels.Import;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemCompleteEvent : IocEvent<LibraryLoaderImportOutputViewModel>
    {
    }
}
