using AudioStation.ViewModels.LibraryViewModels.Comparer;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.RadioViewModels
{
    /// <summary>
    /// Represents a grouping of RadioStationViewModel(s). Most of these will come from the m3u file collection - so
    /// their name will be the name drawn from this file. Otherwise, it will be named after the data service's name.
    /// </summary>
    public class RadioEntryViewModel : ViewModelBase
    {
        string _name;
        SortedObservableCollection<RadioStationViewModel> _stations;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public SortedObservableCollection<RadioStationViewModel> Stations
        {
            get { return _stations; }
            set { this.RaiseAndSetIfChanged(ref _stations, value); }
        }

        public RadioEntryViewModel()
        {
            this.Name = string.Empty;
            this.Stations = new SortedObservableCollection<RadioStationViewModel>(new PropertyComparer<string, RadioStationViewModel>(x => x.Name));
        }
    }
}
