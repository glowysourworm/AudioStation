using AudioStation.ViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemCompleteEvent : IocEvent<LibraryLoaderImportOutputViewModel>
    {
    }
}
