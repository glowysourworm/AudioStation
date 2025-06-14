using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        int _id;
        string _fileName;
        LibraryLoadType _loadType;
        LibraryWorkItemState _loadState;
        string _errorMessage;

        public string FileName
        {
            get { return _fileName; }
            set { RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public LibraryWorkItemState LoadState
        {
            get { return _loadState; }
            set { this.RaiseAndSetIfChanged(ref _loadState, value); }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { this.RaiseAndSetIfChanged(ref _errorMessage, value); }
        }
        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

        public LibraryWorkItemViewModel()
        {
            this.FileName = string.Empty;
            this.ErrorMessage = string.Empty;
        }
    }
}
