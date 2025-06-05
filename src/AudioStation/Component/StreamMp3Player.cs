using System.Windows.Media;

using AudioStation.Component.Interface;
using AudioStation.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component
{
    public class StreamMp3Player : IAudioPlayer
    {
        MediaPlayer _player;

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler PlaybackStoppedEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;        // Not going to use for streams

        public StreamMp3Player()
        {
            _player = new MediaPlayer();
            _player.MediaEnded += OnMediaEnded;
        }

        public void SetVolume(float volume)
        {
            _player.Volume = volume;
        }

        public void Play(string source, StreamSourceType type, int bitrate, string codec)
        {
            if (type != StreamSourceType.Network)
                throw new ArgumentException("Improper use of StreamMp3Player. Expecting network stream type");

            if (_player.HasAudio)
            {
                _player.Stop();
            }

            _player.Open(new Uri(source));
            _player.Play();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Stop()
        {
            _player.Stop();
        }

        public PlaybackState GetPlaybackState()
        {
            if (_player.HasAudio)
                return PlaybackState.Playing;

            return PlaybackState.Stopped;
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            if (this.PlaybackStoppedEvent != null)
                this.PlaybackStoppedEvent();
        }

        public void Dispose()
        {

        }
    }
}
