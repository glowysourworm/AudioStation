﻿using ParkSquare.Discogs;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.Core.Component.Vendor
{
    public class DiscogsClientConfig : ViewModelBase, IClientConfig
    {
        string _authToken;
        string _baseUrl;

        public string AuthToken
        {
            get { return _authToken; }
            set { RaiseAndSetIfChanged(ref _authToken, value); }
        }
        public string BaseUrl
        {
            get { return _baseUrl; }
            set { RaiseAndSetIfChanged(ref _baseUrl, value); }
        }
    }
}
