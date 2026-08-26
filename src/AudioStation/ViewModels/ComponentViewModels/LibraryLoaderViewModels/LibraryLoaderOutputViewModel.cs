using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    /// <summary>
    /// Marker class for library loader base
    /// </summary>
    public class LibraryLoaderOutputViewModel : ViewModelBase
    {
        ViewModelBase _output;

        public ViewModelBase Output
        {
            get { return _output; }
            set { this.RaiseAndSetIfChanged(ref _output, value); }
        }
    }
}
