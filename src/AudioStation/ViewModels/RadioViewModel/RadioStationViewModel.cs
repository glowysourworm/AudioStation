using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.RadioViewModel
{
    public class RadioStationViewModel : ViewModelBase
    {
        string _name;
        string _homepage;
        string _endpoint;
        string _logoEndpoint;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Homepage
        {
            get { return _homepage; }
            set { this.RaiseAndSetIfChanged(ref _homepage, value); }
        }
        public string Endpoint
        {
            get { return _endpoint; }
            set { this.RaiseAndSetIfChanged(ref _endpoint, value); }
        }
        public string LogoEndpoint
        {
            get { return _logoEndpoint; }
            set { this.RaiseAndSetIfChanged(ref _logoEndpoint, value); }
        }

        public RadioStationViewModel()
        {
            this.Name = string.Empty;
            this.Homepage = string.Empty;
            this.Endpoint = string.Empty;
            this.LogoEndpoint = string.Empty;
        }
    }
}
