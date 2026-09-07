using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Load
{
    public class LibraryLoaderFileConverterLoadViewModel : ViewModelBase
    {
        string _fileIn;
        string _fileOut;
        AudioEncoderInfo _encoderInfo;

        public string FileIn
        {
            get { return _fileIn; }
            set { this.RaiseAndSetIfChanged(ref _fileIn, value); }
        }
        public string FileOut
        {
            get { return _fileOut; }
            set { this.RaiseAndSetIfChanged(ref _fileOut, value); }
        }
        public AudioEncoderInfo EncoderInfo
        {
            get { return _encoderInfo; }
            set { this.RaiseAndSetIfChanged(ref _encoderInfo, value); }
        }
    }
}
