using System.Collections.ObjectModel;

using AudioStation.ViewModels.RadioViewModel;

using SimpleWpf.Extensions;

namespace AudioStation.Model
{
    public class Radio : ViewModelBase
    {
        ObservableCollection<RadioEntry> _radioStreams;             // M3U file data
        ObservableCollection<RadioStationViewModel> _radioStations; // Radio Browser / other online services

        public ObservableCollection<RadioEntry> RadioStreams
        {
            get { return _radioStreams; }
            set { this.RaiseAndSetIfChanged(ref _radioStreams, value); }
        }
        public ObservableCollection<RadioStationViewModel> RadioStations
        {
            get { return _radioStations; }
            set { this.RaiseAndSetIfChanged(ref _radioStations, value); }
        }

        public Radio()
        {
            this.RadioStreams = new ObservableCollection<RadioEntry>();
            this.RadioStations = new ObservableCollection<RadioStationViewModel>();
        }
    }
}
