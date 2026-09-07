using CSCore;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
{
    public class AudioEncoderViewModel : ViewModelBase
    {
        string _extension;
        string _filter;
        string _name;
        AudioEncoding _encoding;

        public string Extension
        {
            get { return _extension; }
            set { this.RaiseAndSetIfChanged(ref _extension, value); }
        }
        public string Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public AudioEncoding Encoding
        {
            get { return _encoding; }
            set { this.RaiseAndSetIfChanged(ref _encoding, value); }
        }

        public AudioEncoderViewModel()
        {
            this.Extension = string.Empty;
            this.Filter = string.Empty;
            this.Name = string.Empty;
            this.Encoding = AudioEncoding.Unknown;
        }
    }
}
