using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Controls
{
    public class EqualizerBandViewModel : ViewModelBase
    {
        float _gain;
        float _bandwidth;
        float _frequency;
        int _channels;

        public float Gain
        {
            get { return _gain; }
            set { this.RaiseAndSetIfChanged(ref _gain, value); }
        }
        public float Bandwidth
        {
            get { return _bandwidth; }
            set { this.RaiseAndSetIfChanged(ref _bandwidth, value); }
        }
        public float Frequency
        {
            get { return _frequency; }
            set { this.RaiseAndSetIfChanged(ref _frequency, value); }
        }
        public int Channels
        {
            get { return _channels; }
            set { this.RaiseAndSetIfChanged(ref _channels, value); }
        }

        public EqualizerBandViewModel()
        {

        }
        public EqualizerBandViewModel(float frequency, float gain, float bandwidth, int channels)
        {
            this.Frequency = frequency;
            this.Gain = gain;
            this.Bandwidth = bandwidth;
            this.Channels = channels;
        }
    }
}
