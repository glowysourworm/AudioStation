using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderEntityLoadViewModel<TEntity> : ViewModelBase where TEntity : AudioStationEntityBase
    {
        string _displayName;
        TEntity _entity;

        public TEntity Entity
        {
            get { return _entity; }
            set { this.RaiseAndSetIfChanged(ref _entity, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public LibraryLoaderEntityLoadViewModel()
        {
            this.DisplayName = string.Empty;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
