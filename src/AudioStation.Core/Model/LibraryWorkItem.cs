using AudioStation.Core.Component.LibraryLoaderComponent;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// DTO class for LibraryLoaderWorkItem
    /// </summary>
    public class LibraryWorkItem
    {
        public int Id { get; set; }
        public bool HasErrors { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string LastMessage { get; set; }
        public DateTime EstimatedCompletionTime {  get; set; }
        public LibraryLoadType LoadType { get; set; }
        public LibraryWorkItemState LoadState { get; set; }
        public double PercentComplete { get; set; }
    }
}
