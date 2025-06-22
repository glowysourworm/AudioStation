using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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
        public event SimpleEventHandler<TimeSpan, float[]> PlaybackTickEvent;
        public event SimpleEventHandler PlaybackStoppedEvent;

        MediaFoundationReader _reader;
        IWavePlayer _outputDevice;
        MixingSampleProvider _mixer;
        Equalizer _equalizer;
        EqualizerBand[] _equalizerBands;

        public SimpleMp3PlayerWithEqualizer()
        {
            _reader = null;
            _outputDevice = null;
            _mixer = null;
            _equalizerBands = null;
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
            _outputDevice.Init(_equalizer);
            _outputDevice.PlaybackTick += OnPlaybackTick;
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _outputDevice.PlaybackTickInterval = 10;
            _outputDevice.Volume = 1;
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _outputDevice.Dispose();
                _reader.Dispose();
                _outputDevice.PlaybackTick -= OnPlaybackTick;
                _outputDevice.PlaybackStopped -= OnPlaybackStopped;
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
                // Thread contention will use simple locks on the EqualizerBand
                var currentLevels = new float[_equalizerBands.Length];

                // Be careful here not to call into the lock more than once. The other
                // thread should only be updating sample data.
                for (int index = 0; index < _equalizerBands.Length; index++)
                {
                    var level = 0.0f;

                    for (int channelIndex = 0; channelIndex < _reader.WaveFormat.Channels; channelIndex++)
                    {
                        level += _equalizerBands[index].GetLevelRunningAverage(channelIndex);
                    }

                    currentLevels[index] = level / _reader.WaveFormat.Channels;
                }

                if (this.PlaybackTickEvent != null)
                    this.PlaybackTickEvent(currentTime, currentLevels);
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
            if (_outputDevice != null)
                _outputDevice.Pause();
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
