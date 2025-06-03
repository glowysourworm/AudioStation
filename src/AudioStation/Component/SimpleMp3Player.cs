using System.Diagnostics;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component
{
    /// <summary>
    /// NAudio based Mp3 Player (see their docs)
    /// </summary>
    public class SimpleMp3Player : IDisposable
    {
        private IWavePlayer _wavePlayer;
        private Timer _timer;
        private Stopwatch _stopwatch;                               // Stopwatch will be shared on the timer thread
        private object _lock = new object();
        private const int TIMER_INTERVAL_MILLISECONDS = 1000;
        private const int TIMER_SLEEP_INTERVAL = 100;

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;

        public event SimpleEventHandler PlaybackStoppedEvent;

        public SimpleMp3Player()
        {
            _wavePlayer = null;
            _stopwatch = new Stopwatch();
            _timer = new Timer(new TimerCallback(TimerThread), null, 0, TIMER_INTERVAL_MILLISECONDS);
        }
        ~SimpleMp3Player()
        {
            _timer.Dispose();
            _stopwatch.Stop();
            _wavePlayer.Dispose();
            _wavePlayer = null;
            _timer = null;
        }

        public void SetVolume(float volume)
        {
            _wavePlayer.Volume = Math.Clamp(volume, 0, 1);
        }

        public void Play(string fileName)
        {
            // Stopwatch is shared
            lock (_lock)
            {
                if (_wavePlayer != null)
                {
                    _stopwatch.Restart();
                    _wavePlayer.PlaybackStopped -= OnPlaybackStopped;
                    _wavePlayer.Stop();
                    _wavePlayer.Dispose();
                    _wavePlayer = null;
                }

                var audioFileReader = new AudioFileReader(fileName);
                audioFileReader.Volume = 1;
                _wavePlayer = new WasapiOut();
                _wavePlayer.PlaybackStopped += OnPlaybackStopped;
                _wavePlayer.Init(audioFileReader);
                _wavePlayer.Play();
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                _wavePlayer.Pause();
                _stopwatch.Stop();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _wavePlayer.Stop();
                _stopwatch.Stop();
            }
        }

        private void TimerThread(object? state)
        {
            while (true)
            {
                lock (_lock)
                {
                    if (_stopwatch.IsRunning)
                    {
                        if (this.PlaybackTickEvent != null)
                        {
                            this.PlaybackTickEvent(_stopwatch.Elapsed);
                        }
                    }
                }

                Thread.Sleep(TIMER_SLEEP_INTERVAL);
            }
        }

        public PlaybackState GetPlaybackState()
        {
            if (_wavePlayer == null)
                return PlaybackState.Stopped;

            return _wavePlayer.PlaybackState;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (this.PlaybackStoppedEvent != null)
                this.PlaybackStoppedEvent();
        }

        public void Dispose()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Dispose();
                _wavePlayer = null;
            }
        }
    }
}
