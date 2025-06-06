using AudioStation.Core.Component;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LibraryWorkItemViewModel : ViewModelBase
    {
        string _fileName;
        LibraryLoadType _loadType;
        bool _error;
        bool _completed;

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
        public bool Error
        {
            get { return _error; }
            set { RaiseAndSetIfChanged(ref _error, value); }
        }
        public bool Completed
        {
            get { return _completed; }
            set { RaiseAndSetIfChanged(ref _completed, value); }
        }

        public LibraryWorkItemViewModel()
        {
            this.FileName = string.Empty;
        }
    }
}
