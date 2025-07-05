using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LibraryLoaderViewModels
{
    public class LibraryLoaderImportFileViewModel : ViewModelBase
    {
        string _fileName;
        string _fileFullPath;
        bool _isSelected;
        bool _isMusicBrainzSelected;

        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string FileFullPath
        {
            get { return _fileFullPath; }
            set { this.RaiseAndSetIfChanged(ref _fileFullPath, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public bool IsMusicBrainzSelected
        {
            get { return _isMusicBrainzSelected; }
            set { this.RaiseAndSetIfChanged(ref _isMusicBrainzSelected, value); }
        }

        public LibraryLoaderImportFileViewModel()
        {
            this.FileName = string.Empty;
        }
    }
}
