using System.IO;
using System.Net.Http;

using AudioStation.Component.Interface;
using AudioStation.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component
{
    public class StreamMp3Player : IAudioPlayer
    {
        private IWavePlayer _wavePlayer;
        private BufferedWaveProvider _waveProvider;
        private HttpClient _httpClient;
        private Thread _streamThread;
        private string _streamSource;                   // Http Address of stream
        private object _lock = new object();
        private bool _threadRun = false;
        private const int TIMER_SLEEP_INTERVAL = 10;

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler PlaybackStoppedEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;        // Not going to use for streams

        public StreamMp3Player()
        {
            _waveProvider = new BufferedWaveProvider(new WaveFormat());
            _waveProvider.DiscardOnBufferOverflow = true;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(1);
            _wavePlayer = new WasapiOut();
            _wavePlayer.PlaybackStopped += OnPlaybackStopped;
            _wavePlayer.Init(_waveProvider);

            _threadRun = true;
            _streamThread = new Thread(StreamThreadFunc);
            _streamThread.Start();
        }

        ~StreamMp3Player()
        {
            if (_streamThread != null)
                Dispose();
        }

        public void SetVolume(float volume)
        {
            _wavePlayer.Volume = Math.Clamp(volume, 0, 1);
        }

        public void Play(string source, StreamSourceType type)
        {
            if (type != StreamSourceType.Network)
                throw new ArgumentException("Improper use of StreamMp3Player. Expecting network stream type");

            lock(_lock)
            {
                _streamSource = source;         // Set stream source to new endpoint
                _waveProvider.ClearBuffer();    // Clear the buffer
            }

            _wavePlayer.Play();
        }

        public void Pause()
        {
            _wavePlayer.Pause();
        }

        public void Stop()
        {
            _wavePlayer.Stop();
        }

        private void StreamThreadFunc(object? obj)
        {
            while (_threadRun)
            {
                if (!string.IsNullOrWhiteSpace(_streamSource))
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, _streamSource);
                        var response = _httpClient.Send(request);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseStream = response.Content.ReadAsStream();
                            using (var memoryStream = new MemoryStream())
                            {
                                responseStream.CopyTo(memoryStream);

                                var buffer = memoryStream.GetBuffer();

                                _waveProvider.AddSamples(buffer, 0, buffer.Length);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        int foo = 4;
                    }
                }
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
            if (_streamThread != null)
            {
                _threadRun = false;         // May have been stopped already

                _streamThread.Join(1);
                _streamThread = null;

                _wavePlayer.Dispose();
                _waveProvider = null;
                _wavePlayer = null;
                _httpClient = null;
            }
        }
    }
}
