namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input
{
    public class LibraryLoaderObjectLoadViewModel<T> : LibraryLoaderLoadViewModelBase
    {
        T _load;

        public T Load
        {
            get { return _load; }
            set { this.RaiseAndSetIfChanged(ref _load, value); }
        }

    }
}
