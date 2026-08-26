using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderEntityOutputViewModel<T> : ViewModelBase where T : AudioStationEntityBase
    {
        T _result;

        public T Result
        {
            get { return _result; }
            set { this.RaiseAndSetIfChanged(ref _result, value); }
        }

        public LibraryLoaderEntityOutputViewModel()
        {

        }
    }
}
