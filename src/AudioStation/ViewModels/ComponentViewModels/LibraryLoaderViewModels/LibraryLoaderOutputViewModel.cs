using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    /// <summary>
    /// Marker class for library loader base
    /// </summary>
    public class LibraryLoaderOutputViewModel : ViewModelBase
    {
        object _payload;

        public object Payload
        {
            get { return _payload; }
            set { this.RaiseAndSetIfChanged(ref _payload, value); }
        }
    }
}
