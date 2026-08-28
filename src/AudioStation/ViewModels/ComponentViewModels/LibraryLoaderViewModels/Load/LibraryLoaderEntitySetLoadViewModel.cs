using System.Collections.ObjectModel;

using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderEntitySetLoadViewModel<TEntity> : ViewModelBase where TEntity : AudioStationEntityBase
    {
        ObservableCollection<TEntity> _entitySet;

        public ObservableCollection<TEntity> EntitySet
        {
            get { return _entitySet; }
            set { this.RaiseAndSetIfChanged(ref _entitySet, value); }
        }

        public LibraryLoaderEntitySetLoadViewModel()
        {
            this.EntitySet = new ObservableCollection<TEntity>();
        }
    }
}
