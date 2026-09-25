using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Model;
using AudioStation.Model.AudioProcessing.Interface;

using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams.Effects;

using SimpleWpf.Extensions.Event;
using SimpleWpf.Utilities;

namespace AudioStation.Model.AudioProcessing
{
    public class SimpleAudioPlayerWithEqualizer : IAudioPlayer
    {
        public bool HasAudio { get; }

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        public event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;
        public event SimpleEventHandler<StoppedEventArgs> PlaybackStoppedEvent;

        // Platform Specifics
        ISoundOut? _soundOut;

        // Codec -> Sample Source -> Effects Chain -> Wave Source
        IWaveSource? _waveSource;
        Equalizer? _equalizer;

        EqualizerResultSet _equalizerResult;

        DispatcherTimer _timer;

        public SimpleAudioPlayerWithEqualizer()
        {
            _soundOut = null;
            _waveSource = null;
            _equalizer = null;

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

            if (_waveSource != null)
                Dispose();


            // Source (+ Effects Chain)
            _waveSource = CodecFactory.Instance.GetCodec(fileSource)
                                      .Loop()
                                      .ToSampleSource()
                                      .AppendSource(Equalizer.Create10BandEqualizer, out _equalizer)
                                      .ToWaveSource();

            // Output (WASAPI, MMF, Direct Sound, ASIO)
            //
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                _soundOut = new WasapiOut();

            else
                _soundOut = new DirectSoundOut();

            _soundOut.Initialize(_waveSource);
            _soundOut.Stopped += OnPlaybackStopped;
        }

        public void Dispose()
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(Dispose, DispatcherPriority.Background);

            else if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (_soundOut != null)
                {
                    _soundOut.Stop();
                    _soundOut.Dispose();
                    _timer.IsEnabled = false;
                    _equalizer.Dispose();
                    _waveSource.Dispose();

                    _soundOut.Stopped -= OnPlaybackStopped;
                    _soundOut = null;
                    _equalizer = null;
                    _waveSource = null;
                }
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(OnPlaybackStopped, DispatcherPriority.Background, sender, e);

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
                this.PlaybackTickEvent(_waveSource == null ? TimeSpan.Zero : _waveSource.GetTime(_waveSource.Position));

            // Contends for FFT result from NAudio
            //var fftResult = _aggregator.FFTResult;

            // Update our result set
            //var update = _equalizerResult.Update(fftResult);

            //if (update != EqualizerResultSet.UpdateType.None)
            //{
            //    if (this.EqualizerCalculated != null)
            //        this.EqualizerCalculated(_equalizerResult);
            //}
        }

        public PlaybackState GetPlaybackState()
        {
            if (_soundOut != null)
                return _soundOut.PlaybackState;

            return PlaybackState.Stopped;
        }

        public float GetVolume()
        {
            if (_soundOut != null)
                return _soundOut.Volume;

            return 0;
        }

        public void Pause()
        {
            _soundOut?.Pause();
            _timer.IsEnabled = false;
        }

        public void Play(string source, StreamSourceType sourceType)
        {
            if (_soundOut == null || _soundOut.PlaybackState != PlaybackState.Stopped)
            {
                CreateDevice(source);
            }

            _soundOut.Play();
            _timer.IsEnabled = true;
        }

        public void Resume()
        {
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Playing)
            {
                _soundOut.Play();
                _timer.IsEnabled = true;
            }
        }

        public void SetPosition(TimeSpan position)
        {
            if (_soundOut != null)
            {
                _waveSource.SetPosition(position);
            }
        }
        public void SetPosition(float positionRatio)
        {
            if (_waveSource != null)
            {
                var totalTime = _waveSource.GetTime(_waveSource.Length);
                var positionMilliseconds = totalTime.TotalMilliseconds * positionRatio;

                _waveSource.SetPosition(TimeSpan.FromMilliseconds(positionMilliseconds));
            }
        }
        public TimeSpan GetDuration()
        {
            if (_waveSource != null)
            {
                return _waveSource.GetTime(_waveSource.Length);
            }
            else
                return TimeSpan.Zero;
        }
        public void SetVolume(float volume)
        {
            if (_soundOut != null)
                _soundOut.Volume = volume;
        }

        public void SetEqualizerGain(float frequency, float gain)
        {
            //for (int index = 0; index < _equalizerBands.Length; index++)
            //{
            //    // Set Gain in decibels (go ahead and use linear scale) (also, these are not lock-protected, but it's just a float setting)
            //    if (_equalizerBands[index].Frequency == frequency)
            //        _equalizerBands[index].Gain = (float)AudioMath.ToDecibel(gain, AudioScale.AudioScaleType.GainStandardLogarithmic);
            //}

            //_equalizer.Update();
        }

        public void Stop()
        {
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Stopped)
                _soundOut.Stop();

            _timer.IsEnabled = false;
        }
    }
}
