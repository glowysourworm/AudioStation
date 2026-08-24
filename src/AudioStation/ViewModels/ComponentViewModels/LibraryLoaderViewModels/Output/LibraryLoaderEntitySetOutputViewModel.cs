using System.Collections.ObjectModel;

using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Output
{
    public class LibraryLoaderEntitySetOutputViewModel<T> : LibraryLoaderOutputViewModelBase where T : AudioStationEntityBase
    {
        ObservableCollection<T> _resultSet;

        public ObservableCollection<T> ResultSet
        {
            get { return _resultSet; }
            set { this.RaiseAndSetIfChanged(ref _resultSet, value); }
        }

        public LibraryLoaderEntitySetOutputViewModel()
        {
            this.ResultSet = new ObservableCollection<T>();
        }
    }
}
