using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderFileLoadViewModel : ViewModelBase
    {
        string _fullPath;
        string _shortPath;

        public string FullPath
        {
            get { return _fullPath; }
            set { this.RaiseAndSetIfChanged(ref _fullPath, value); }
        }
        public string ShortPath
        {
            get { return _shortPath; }
            set { this.RaiseAndSetIfChanged(ref _shortPath, value); }
        }


        public LibraryLoaderFileLoadViewModel(string fullPath, string shortPath)
        {
            this.FullPath = fullPath;
            this.ShortPath = shortPath;
        }

        public override string ToString()
        {
            return _shortPath;
        }
    }
}
