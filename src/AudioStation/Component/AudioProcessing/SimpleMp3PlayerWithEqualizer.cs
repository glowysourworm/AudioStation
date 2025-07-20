using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.AudioProcessing.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using NAudio.Extras;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component.AudioProcessing
{
    public class SimpleMp3PlayerWithEqualizer : IAudioPlayer
    {
        public bool HasAudio { get; }

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        public event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;
        public event SimpleEventHandler<StoppedEventArgs> PlaybackStoppedEvent;

        MediaFoundationReader _reader;
        IWavePlayer _outputDevice;
        MixingSampleProvider _mixer;
        SampleAggregator _aggregator;
        Equalizer _equalizer;
        EqualizerBand[] _equalizerBands;

        EqualizerResultSet _equalizerResult;

        DispatcherTimer _timer;

        public SimpleMp3PlayerWithEqualizer()
        {
            _reader = null;
            _outputDevice = null;
            _aggregator = null;
            _mixer = null;
            _equalizer = null;
            _equalizerBands = null;
            _equalizerResult = null;

            _timer = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
            _timer.Tick += OnTimerTick;
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Start();
            _timer.IsEnabled = false;
        }

        private void CreateDevice(string fileSource)
        {
            if (string.IsNullOrEmpty(fileSource) ||
                !fileSource.EndsWith(".mp3"))
                throw new ArgumentException("NAudio media source must be an .mp3 file with the proper file extension");

            if (_reader != null)
                Dispose();

            _reader = new MediaFoundationReader(fileSource, new MediaFoundationReader.MediaFoundationReaderSettings()
            {
                RequestFloatOutput = true
            });
            _outputDevice = new WaveOutEvent();

            var sampleProvider = _reader.ToSampleProvider();

            //_mixer = new MixingSampleProvider(_reader.WaveFormat);      // Use MediaFoundationReader to analyze the stream
            //_mixer.AddMixerInput(sampleProvider);
            //_mixer.AddMixerInput(_equalizer);

            var bandWidth = 0.8f;
            var gainDB = 0;

            _equalizerBands = new EqualizerBand[]
            {
                new EqualizerBand(100, gainDB, bandWidth),
                new EqualizerBand(200, gainDB, bandWidth),
                new EqualizerBand(400, gainDB, bandWidth),
                new EqualizerBand(800, gainDB, bandWidth),
                new EqualizerBand(1200, gainDB, bandWidth),
                new EqualizerBand(2400, gainDB, bandWidth),
                new EqualizerBand(4800, gainDB, bandWidth),
                new EqualizerBand(9600, gainDB, bandWidth)
            };
            _equalizer = new Equalizer(sampleProvider, _equalizerBands);
            _equalizerResult = new EqualizerResultSet((int)Math.Pow(2, 10), (int)Math.Pow(2, 7), 1, 25, 0.75f);
            _aggregator = new SampleAggregator(_equalizer, _equalizerResult.ChannelsInput);                     // "Channels" is a misnomer. The FFT "buffer" has to be
            _outputDevice.Init(_aggregator);
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _outputDevice.Volume = 1;
        }

        public void Dispose()
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(Dispose, DispatcherPriority.Background);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (_reader != null)
                {
                    _timer.IsEnabled = false;
                    _outputDevice.Stop();
                    _outputDevice.Dispose();
                    _reader.Dispose();
                    _outputDevice.PlaybackStopped -= OnPlaybackStopped;
                    _aggregator = null;
                    _equalizer = null;
                    _equalizerBands = null;
                    _reader = null;
                    _outputDevice = null;
                }
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(OnPlaybackStopped, DispatcherPriority.Background, sender, e);

            else
            {
                if (this.PlaybackStoppedEvent != null)
                    this.PlaybackStoppedEvent(e);
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // Dispatcher Timer => Dispatcher Thread. So, go ahead and fire the event.
            if (this.PlaybackTickEvent != null)
                this.PlaybackTickEvent(_reader == null ? TimeSpan.Zero : _reader.CurrentTime);

            // Contends for FFT result from NAudio
            var fftResult = _aggregator.FFTResult;

            // Update our result set
            var update = _equalizerResult.Update(fftResult);

            if (update != EqualizerResultSet.UpdateType.None)
            {
                if (this.EqualizerCalculated != null)
                    this.EqualizerCalculated(_equalizerResult);
            }
        }

        public PlaybackState GetPlaybackState()
        {
            if (_outputDevice != null)
                return _outputDevice.PlaybackState;

            return PlaybackState.Stopped;
        }

        public float GetVolume()
        {
            if (_outputDevice != null)
                return _outputDevice.Volume;

            return 0;
        }

        public void Pause()
        {
            _outputDevice?.Pause();
            _timer.IsEnabled = false;
        }

        public void Play(string source, StreamSourceType sourceType)
        {
            if (_outputDevice == null || _outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                CreateDevice(source);
            }
                
            _outputDevice.Play();
            _timer.IsEnabled = true;
        }

        public void Resume()
        {
            if (_outputDevice != null && _outputDevice.PlaybackState != PlaybackState.Playing)
            {
                _outputDevice.Play();
                _timer.IsEnabled = true;
            }
        }

        public void SetPosition(TimeSpan position)
        {
            if (_outputDevice != null)
            {
                var unitPosition = position.TotalMilliseconds / (float)_reader.TotalTime.TotalMilliseconds;
                var bytePosition = (long)(unitPosition * _reader.Length);

                var byteDelta = bytePosition - _reader.Position;

                _reader.Skip((int)(position.TotalSeconds - _reader.CurrentTime.TotalSeconds));
            }
        }

        public void SetVolume(float volume)
        {
            if (_outputDevice != null)
                _outputDevice.Volume = volume;
        }

        public void SetEqualizerGain(float frequency, float gain)
        {
            for (int index = 0; index < _equalizerBands.Length; index++)
            {
                // Set Gain in decibels (go ahead and use linear scale) (also, these are not lock-protected, but it's just a float setting)
                if (_equalizerBands[index].Frequency == frequency)
                    _equalizerBands[index].Gain = (float)AudioMath.ToDecibel(gain, AudioScale.AudioScaleType.GainStandardLogarithmic); 
            }

            _equalizer.Update();
        }

        public void Stop()
        {
            if (_outputDevice != null && _outputDevice.PlaybackState != PlaybackState.Stopped)
                _outputDevice.Stop();

            _timer.IsEnabled = false;
        }
    }
}
