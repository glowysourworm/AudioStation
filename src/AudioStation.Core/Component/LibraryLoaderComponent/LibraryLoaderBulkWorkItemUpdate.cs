namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// Bulk work item progress report
    /// </summary>
    public class LibraryLoaderBulkWorkItemUpdate
    {
        public int Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public LibraryLoadType Type { get; set; }
        public LibraryWorkItemState State { get; set; }
        public double Progress { get; set; }
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
        public int CompletedCount { get; set; }

        public int DataErrorCount { get; set; }
        public int DataWarningCount { get; set; }
        public int ServiceFailureCount { get; set; }
        public int ServiceNoResultCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }

        public int TotalCount { get; set; }

        public LibraryLoaderBulkWorkItemUpdate(int id, Guid ownerId)
        {
            this.Id = id;
            this.OwnerId = ownerId;
        }
    }
}
