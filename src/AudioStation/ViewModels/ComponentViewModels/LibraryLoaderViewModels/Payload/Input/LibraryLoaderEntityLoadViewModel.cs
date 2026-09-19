using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Payload.Input
{
    public class LibraryLoaderEntityLoadViewModel<TEntity> : ViewModelBase where TEntity : AudioStationEntityBase
    {
        TEntity _entity;

        public TEntity Entity
        {
            get { return _entity; }
            set { this.RaiseAndSetIfChanged(ref _entity, value); }
        }

        public LibraryLoaderEntityLoadViewModel()
        {
        }
    }
}
