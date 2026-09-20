using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    /// <summary>
    /// Marker class for library loader input load
    /// </summary>
    public class LibraryLoaderLoadViewModel : ViewModelBase, ILibraryLoaderLoad
    {
        int _ownerId;
        LibraryLoadType _loadType;
        object _payload;
        string _displayName;

        public int OwnerId
        {
            get { return _ownerId; }
            set { this.RaiseAndSetIfChanged(ref _ownerId, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }
        public LibraryLoadType LoadType
        {
            get { return _loadType; }
            set { this.RaiseAndSetIfChanged(ref _loadType, value); }
        }
        public object Payload
        {
            get { return _payload; }
            set { this.RaiseAndSetIfChanged(ref _payload, value); }
        }
    }
}
