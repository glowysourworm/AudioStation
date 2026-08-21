using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemUpdateEvent : IocEvent<LibraryWorkItemViewModel>
    {
    }
}
