using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.RadioViewModels
{
    public class RadioStationViewModel : ViewModelBase
    {
        string _name;
        string _homepage;
        string _endpoint;
        string _logoEndpoint;

        int _bitrate;
        string _codec;

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
        public int Bitrate
        {
            get { return _bitrate; }
            set { this.RaiseAndSetIfChanged(ref _bitrate, value); }
        }
        public string Codec
        {
            get { return _codec; }
            set { this.RaiseAndSetIfChanged(ref _codec, value); }
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
