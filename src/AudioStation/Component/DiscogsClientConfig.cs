using ParkSquare.Discogs;

using SimpleWpf.Extensions;

namespace AudioStation.Component
{
    public class DiscogsClientConfig : ViewModelBase, IClientConfig
    {
        string _authToken;
        string _baseUrl;

        public string AuthToken
        {
            get { return _authToken; }
            set { this.RaiseAndSetIfChanged(ref _authToken, value); }
        }
        public string BaseUrl
        {
            get { return _baseUrl; }
            set { this.RaiseAndSetIfChanged(ref _baseUrl, value); }
        }
    }
}
