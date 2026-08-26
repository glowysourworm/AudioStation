using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    /// <summary>
    /// Marker class for library loader input load
    /// </summary>
    public class LibraryLoaderLoadViewModel : ViewModelBase
    {
        ViewModelBase _data;
        string _displayText;

        public ViewModelBase Data
        {
            get { return _data; }
            set { this.RaiseAndSetIfChanged(ref _data, value); }
        }
        public string DisplayText
        {
            get { return _displayText; }
            set { this.RaiseAndSetIfChanged(ref _displayText, value); }
        }
    }
}
