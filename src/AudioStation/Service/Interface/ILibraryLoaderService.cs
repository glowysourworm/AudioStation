using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        /// <summary>
        /// Work item that is initialized by another component. The load type will designate how to 
        /// handle the load / output types. The method will return an ID from the ILibraryLoader on
        /// the back end. This should be stored as your ID in the work item and kept to reference
        /// during updates.
        /// </summary>
        int RunLoaderTaskAsync(LibraryWorkItemViewModel workItem);
    }
}
