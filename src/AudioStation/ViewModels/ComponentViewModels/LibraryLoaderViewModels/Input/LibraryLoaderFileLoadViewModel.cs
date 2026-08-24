namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input
{
    public class LibraryLoaderFileLoadViewModel : LibraryLoaderLoadViewModelBase
    {
        string _file;

        public string File
        {
            get { return _file; }
            set { this.RaiseAndSetIfChanged(ref _file, value); }
        }

        public LibraryLoaderFileLoadViewModel(string file)
        {
            this.File = file;
        }
    }
}
