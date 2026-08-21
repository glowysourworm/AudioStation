using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemCompleteEvent : IocEvent<LibraryImporterOutputViewModel>
    {
    }
}
