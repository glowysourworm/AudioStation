using System.Collections.ObjectModel;

using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels
{
    public class LibraryBulkWorkItemViewModel : ViewModelBase
    {
        int _id;
        Guid _ownerId;
        string _description;
        double _progress;
        int _pendingCount;
        int _processingCount;
        int _completedCount;
        int _dataErrorCount;
        int _dataWarningCount;
        int _serviceFailureCount;
        int _serviceNoResultCount;
        int _successCount;
        int _failureCount;
        int _totalCount;

        ObservableCollection<LibraryWorkItemViewModel> _workItemsPending;
        ObservableCollection<LibraryWorkItemViewModel> _workItemsProcessing;
        ObservableCollection<LibraryWorkItemViewModel> _workItemsCompleted;

        LibraryLoadType _loadType;
        LibraryWorkItemState _state;

        // UI Properties
        bool _isSelected;

        public int Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public Guid OwnerId
        {
            get { return _ownerId; }
            private set { this.RaiseAndSetIfChanged(ref _ownerId, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
        public int PendingCount
        {
            get { return _pendingCount; }
            set { this.RaiseAndSetIfChanged(ref _pendingCount, value); }
        }
        public int ProcessingCount
        {
            get { return _processingCount; }
            set { this.RaiseAndSetIfChanged(ref _processingCount, value); }
        }
        public int CompletedCount
        {
            get { return _completedCount; }
            set { this.RaiseAndSetIfChanged(ref _completedCount, value); }
        }
        public int DataErrorCount
        {
            get { return _dataErrorCount; }
            set { this.RaiseAndSetIfChanged(ref _dataErrorCount, value); }
        }
        public int DataWarningCount
        {
            get { return _dataWarningCount; }
            set { this.RaiseAndSetIfChanged(ref _dataWarningCount, value); }
        }
        public int ServiceFailureCount
        {
            get { return _serviceFailureCount; }
            set { this.RaiseAndSetIfChanged(ref _serviceFailureCount, value); }
        }
        public int ServiceNoResultCount
        {
            get { return _serviceNoResultCount; }
            set { this.RaiseAndSetIfChanged(ref _serviceNoResultCount, value); }
        }
        public int SuccessCount
        {
            get { return _successCount; }
            set { this.RaiseAndSetIfChanged(ref _successCount, value); }
        }
        public int FailureCount
        {
            get { return _failureCount; }
            set { this.RaiseAndSetIfChanged(ref _failureCount, value); }
        }
        public int TotalCount
        {
            get { return _totalCount; }
            set { this.RaiseAndSetIfChanged(ref _totalCount, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> WorkItemsPending
        {
            get { return _workItemsPending; }
            set { this.RaiseAndSetIfChanged(ref _workItemsPending, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> WorkItemsProcessing
        {
            get { return _workItemsProcessing; }
            set { this.RaiseAndSetIfChanged(ref _workItemsProcessing, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> WorkItemsCompleted
        {
            get { return _workItemsCompleted; }
            set { this.RaiseAndSetIfChanged(ref _workItemsCompleted, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { this.RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public LibraryWorkItemState State
        {
            get { return _state; }
            set { this.RaiseAndSetIfChanged(ref _state, value); }
        }

        public LibraryBulkWorkItemViewModel(int id, Guid ownerId, string description)
        {
            this.Id = id;
            this.OwnerId = ownerId;
            this.Description = description;
            this.WorkItemsCompleted = new ObservableCollection<LibraryWorkItemViewModel>();
            this.WorkItemsPending = new ObservableCollection<LibraryWorkItemViewModel>();
            this.WorkItemsProcessing = new ObservableCollection<LibraryWorkItemViewModel>();
        }
    }
}
