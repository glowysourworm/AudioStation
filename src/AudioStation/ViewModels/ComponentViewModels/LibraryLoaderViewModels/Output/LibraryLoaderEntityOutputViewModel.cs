using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderEntityOutputViewModel<T> : LibraryLoaderOutputViewModelBase where T : AudioStationEntityBase
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
