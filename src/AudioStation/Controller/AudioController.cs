using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Event;

using NAudio.Wave;

namespace AudioStation.Controller
{
    public class AudioController : IAudioController
    {
        public event SimpleEventHandler PlaybackStoppedEvent;

        private SimpleMp3Player _player;

        public AudioController()
        {
            _player = new SimpleMp3Player();
            _player.PlaybackStoppedEvent += PlaybackStoppedEvent;
        }

        public PlaybackState GetPlaybackState()
        {
            return _player.GetPlaybackState();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Play(string fileName)
        {
            _player.Play(fileName);
        }

        public void Stop()
        {
            _player.Stop();
        }
    }
}
