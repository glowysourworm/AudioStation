using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    /// <summary>
    /// Marker class for library loader input load
    /// </summary>
    public class LibraryLoaderLoadViewModel : ViewModelBase
    {
        object _payload;

        public object Payload
        {
            get { return _payload; }
            set { this.RaiseAndSetIfChanged(ref _payload, value); }
        }
    }
}
