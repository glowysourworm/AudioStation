using System.Collections.ObjectModel;

using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Input
{
    public class LibraryLoaderEntitySetLoadViewModel<TEntity> : LibraryLoaderLoadViewModelBase where TEntity : AudioStationEntityBase
    {
        string _displayName;
        ObservableCollection<TEntity> _entitySet;

        public ObservableCollection<TEntity> EntitySet
        {
            get { return _entitySet; }
            set { this.RaiseAndSetIfChanged(ref _entitySet, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public LibraryLoaderEntitySetLoadViewModel()
        {
            this.EntitySet = new ObservableCollection<TEntity>();
            this.DisplayName = string.Empty;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
