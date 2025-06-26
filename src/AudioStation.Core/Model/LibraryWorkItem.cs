using AudioStation.Core.Component.LibraryLoaderComponent;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// DTO class for LibraryLoaderWorkItem
    /// </summary>
    public class LibraryWorkItem
    {
        public int Id { get; set; }
        public LibraryLoadType LoadType { get; set; }
        public LibraryWorkItemState LoadState { get; set; }
        public double PercentComplete { get; set; }
    }
}
