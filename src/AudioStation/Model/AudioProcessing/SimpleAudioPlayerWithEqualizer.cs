using System.Numerics;
using System.Windows.Threading;

using AudioStation.Core.Model;
using AudioStation.Core.Utility.MathUtility;
using AudioStation.Model.AudioProcessing.Interface;

using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
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
        NotificationSource _equalizerNotifier;
        Complex[] _fftBuffer;
        int _fftIndex;
        int _fftPeriod;

        public SimpleAudioPlayerWithEqualizer()
        {
            _soundOut = null;
            _waveSource = null;
            _equalizer = null;

            _fftPeriod = 1024;
            _equalizerResult = new EqualizerResultSet(_fftPeriod, _fftPeriod, 1, 20, 0.80f);
            _fftBuffer = new Complex[_fftPeriod];
            _fftIndex = 0;
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
                                      .AppendSource(equalizer => new NotificationSource(equalizer), out _equalizerNotifier)
                                      .ToWaveSource();

            // Equalizer Updater
            _equalizerNotifier.BlockRead += OnEquzlierNotifierRead;

            // Output (WASAPI, MMF, Direct Sound, ASIO)
            //
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                _soundOut = new WasapiOut();

            else
                _soundOut = new DirectSoundOut();

            _soundOut.Initialize(_waveSource);
            _soundOut.Stopped += OnPlaybackStopped;
        }

        private void OnEquzlierNotifierRead(object? sender, BlockReadEventArgs<float> e)
        {
            // Use opportunity to update the current time
            if (this.PlaybackTickEvent != null && _waveSource != null)
                this.PlaybackTickEvent(_waveSource.GetTime(_waveSource.Position));

            // FFT Buffer is circular with its own index
            //
            for (int index = 0; index < e.Length; index++)
            {
                // Window Function:  This will help define the frequency spectrum. Typically this 
                //                   shaves off some of the intensity at the ends.
                //
                var windowValue = FFTAlgorithm.GaussianWindow(0.1, _fftIndex, _fftPeriod);

                _fftBuffer[_fftIndex] = windowValue * e.Data[index];

                // Independent Circular Index
                _fftIndex++;

                if (_fftIndex >= _fftPeriod)
                {
                    // Reset Index
                    _fftIndex = 0;

                    // Calculate FFT
                    FFTAlgorithm.FFT(_fftBuffer);

                    // Update Result Integrator
                    var update = _equalizerResult.Update(_fftBuffer);

                    if (update != EqualizerResultSet.UpdateType.None)
                    {
                        if (this.EqualizerCalculated != null)
                            this.EqualizerCalculated(_equalizerResult);
                    }
                }
            }
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
                    _equalizer.Dispose();
                    _waveSource.Dispose();
                    _equalizerNotifier.Dispose();

                    _soundOut.Stopped -= OnPlaybackStopped;
                    _soundOut = null;
                    _equalizer = null;
                    _waveSource = null;

                    _equalizerNotifier.BlockRead -= OnEquzlierNotifierRead;
                    _equalizerNotifier = null;
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
        }

        public void Play(string source, StreamSourceType sourceType)
        {
            if (_soundOut == null || _soundOut.PlaybackState != PlaybackState.Stopped)
            {
                CreateDevice(source);
            }

            _soundOut.Play();
        }

        public void Resume()
        {
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Playing)
            {
                _soundOut.Play();
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
        }
    }
}
