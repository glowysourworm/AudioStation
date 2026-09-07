using AudioStation.Core.Model.Interface;

using CSCore;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
{
    public class AudioEncoderViewModel : ViewModelBase, IAudioEncoderInfo
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

        public override bool Equals(object? obj)
        {
            var other = obj as AudioEncoderViewModel;

            if (other == null)
                return false;

            return other.Encoding == this.Encoding &&
                other.Extension == this.Extension &&
                other.Filter == this.Filter &&
                other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Encoding, this.Extension, this.Filter, this.Name);
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
