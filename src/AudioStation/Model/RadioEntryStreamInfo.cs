using SimpleWpf.Extensions;

namespace AudioStation.Model
{
    public class RadioEntryStreamInfo : ViewModelBase
    {
        string _title;
        string _endpoint;
        string _logoEndpoint;

        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
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


        public RadioEntryStreamInfo()
        {
            this.Title = string.Empty;
            this.Endpoint = string.Empty;
            this.LogoEndpoint = string.Empty;
        }
    }
}
