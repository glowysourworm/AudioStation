using AudioStation.ViewModels.LibraryLoaderViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemCompleteEvent : IocEvent<LibraryLoaderImportOutputViewModel>
    {
    }
}
