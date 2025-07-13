using AudioStation.ViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event.LibraryLoaderEvent
{
    public class LibraryLoaderWorkItemUpdateEvent : IocEvent<LibraryWorkItemViewModel>
    {
    }
}
