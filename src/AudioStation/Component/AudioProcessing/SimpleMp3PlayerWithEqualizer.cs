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

            var bandWidth = 0.8f;
            var gainDB = 0;

            _equalizerBands = new EqualizerBand[]
            {
                new EqualizerBand(100, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(200, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(400, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(800, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(1200, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(2400, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(4800, gainDB, bandWidth, _reader.WaveFormat.Channels),
                new EqualizerBand(9600, gainDB, bandWidth, _reader.WaveFormat.Channels)
            };
            _equalizer = new Equalizer(sampleProvider, _equalizerBands);
            _equalizerResult = new EqualizerResultSet((int)Math.Pow(2, 10), (int)Math.Pow(2, 7), 5, 5, 0.3f);
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
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(Dispose, DispatcherPriority.Background);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (_reader != null)
                {
                    _aggregator.Dispose();
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
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnPlaybackStopped, DispatcherPriority.Background, sender, e);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.PlaybackStoppedEvent != null)
                    this.PlaybackStoppedEvent(e);
            }
        }

        private void OnPlaybackTick(object? sender, TimeSpan currentTime)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnPlaybackTick, DispatcherPriority.Background, sender, currentTime);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.PlaybackTickEvent != null)
                    this.PlaybackTickEvent(_reader == null ? TimeSpan.Zero : _reader.CurrentTime);
            }
        }

        private void OnFFTCalculated(object? sender, FftEventArgs fftArgs)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnFFTCalculated, DispatcherPriority.Background, sender, fftArgs);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
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
            if (_outputDevice == null || _outputDevice.PlaybackState != PlaybackState.Stopped)
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
                // Set Gain in decibels (go ahead and use linear scale)
                if (_equalizerBands[index].GetFrequency() == frequency)
                    _equalizerBands[index].UpdateParameters(((gain * 2) - 1) * 20.0f); // Must add an offset (hearing threshold) to keep it off zero
            }

            _equalizer.Update();
        }

        public void Stop()
        {
            if (_outputDevice != null && _outputDevice.PlaybackState != PlaybackState.Stopped)
                _outputDevice.Stop();
        }
    }
}
