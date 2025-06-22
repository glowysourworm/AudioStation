using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.AudioProcessing;
using AudioStation.Component.Interface;
using AudioStation.Core.Model;

using NAudio.Extras;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component
{
    public class SimpleMp3PlayerWithEqualizer : IAudioPlayer
    {
        public bool HasAudio { get; }

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        public event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;
        public event SimpleEventHandler PlaybackStoppedEvent;

        MediaFoundationReader _reader;
        IWavePlayer _outputDevice;
        MixingSampleProvider _mixer;
        SampleAggregator _aggregator;
        Equalizer _equalizer;
        EqualizerBand[] _equalizerBands;

        EqualizerResultSet _equalizerResult;

        public SimpleMp3PlayerWithEqualizer()
        {
            _reader = null;
            _outputDevice = null;
            _aggregator = null;
            _mixer = null;
            _equalizer = null;
            _equalizerBands = null;
            _equalizerResult = null;
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

            _equalizerBands = new EqualizerBand[]
            {
                new EqualizerBand(100, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(200, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(400, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(800, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(1200, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(2400, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(4800, 1, 0.8f, _reader.WaveFormat.Channels),
                new EqualizerBand(9600, 1, 0.8f, _reader.WaveFormat.Channels)
            };
            _equalizer = new Equalizer(sampleProvider, _equalizerBands);
            _equalizerResult = new EqualizerResultSet((int)Math.Pow(2, 10), (int)Math.Pow(2, 7), 10, 10, 0.3f);
            _aggregator = new SampleAggregator(_equalizer, _equalizerResult.ChannelsInput);                     // "Channels" is a misnomer. The FFT "buffer" has to be
            _aggregator.PerformFFT = true;                                                                      // related to the frequency
            _aggregator.FftCalculated += OnFFTCalculated;
            _outputDevice.Init(_aggregator);
            _outputDevice.PlaybackTick += OnPlaybackTick;
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _outputDevice.PlaybackTickInterval = 10;
            _outputDevice.Volume = 1;
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _outputDevice.Stop();
                _outputDevice.Dispose();
                _reader.Dispose();
                _outputDevice.PlaybackTick -= OnPlaybackTick;
                _outputDevice.PlaybackStopped -= OnPlaybackStopped;
                _aggregator.FftCalculated -= OnFFTCalculated;
                _aggregator = null;
                _equalizer = null;
                _equalizerBands = null;
                _reader = null;
                _outputDevice = null;
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnPlaybackStopped, DispatcherPriority.Background, sender, e);
            else
            {
                if (this.PlaybackStoppedEvent != null)
                    this.PlaybackStoppedEvent();
            }
        }

        private void OnPlaybackTick(object? sender, TimeSpan currentTime)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnPlaybackTick, DispatcherPriority.Background, sender, currentTime);
            else
            {
                if (this.PlaybackTickEvent != null)
                    this.PlaybackTickEvent(currentTime);
            }
        }

        private void OnFFTCalculated(object? sender, FftEventArgs fftArgs)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnFFTCalculated, DispatcherPriority.Background, sender, fftArgs);
            else
            {
                var update = _equalizerResult.Update(fftArgs.Result);

                if (update != EqualizerResultSet.UpdateType.None)
                {
                    if (this.EqualizerCalculated != null)
                        this.EqualizerCalculated(_equalizerResult);
                }
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
        }

        public void Play(string source, StreamSourceType sourceType)
        {
            CreateDevice(source);

            _outputDevice.Play();
        }

        public void Resume()
        {
            if (_outputDevice != null && _outputDevice.PlaybackState != PlaybackState.Playing)
                _outputDevice.Play();
        }

        public void SetPosition(TimeSpan position)
        {
            if (_outputDevice != null)
            {
                var unitPosition = position.TotalMilliseconds / (float)_reader.TotalTime.TotalMilliseconds;
                var bytePosition = (long)(unitPosition * _reader.Length);

                _reader.Position = bytePosition;
            }
        }

        public void SetVolume(float volume)
        {
            if (_outputDevice != null)
                _outputDevice.Volume = volume;
        }

        public void Stop()
        {
            if (_outputDevice != null && _outputDevice.PlaybackState != PlaybackState.Stopped)
                _outputDevice.Stop();
        }
    }
}
