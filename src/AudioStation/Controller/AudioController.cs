using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.ViewModels.Interface;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller
{
    public class AudioController : IAudioController
    {
        public event SimpleEventHandler<INowPlayingViewModel> PlaybackStartedEvent;
        public event SimpleEventHandler<INowPlayingViewModel> PlaybackStoppedEvent;

        private IAudioPlayer _player;
        private INowPlayingViewModel _nowPlaying;

        public AudioController()
        {
            _player = null;
        }

        ~AudioController()
        {
            if(_player != null)
            {
                _player.Dispose();
                _player.PlaybackStoppedEvent -= OnPlaybackStopped;
                _player.PlaybackTickEvent -= OnPlaybackTick;
                _player = null;
            }
        }

        public PlaybackState GetPlaybackState()
        {
            return _player.GetPlaybackState();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Play(INowPlayingViewModel nowPlaying)
        {
            StopImpl();

            _nowPlaying = nowPlaying;

            StartImpl();
        }

        public void Stop()
        {
            StopImpl();
        }

        private void StartImpl()
        {
            if (_player != null &&
               (_player.GetPlaybackState() == PlaybackState.Playing ||
                _player.GetPlaybackState() == PlaybackState.Paused))
            {
                return;
            }

            if (!string.IsNullOrEmpty(_nowPlaying.Source))
            {
                if (_player == null)
                {
                    switch (_nowPlaying.SourceType)
                    {
                        case Model.StreamSourceType.File:
                            _player = new SimpleMp3Player();
                            break;
                        case Model.StreamSourceType.Network:
                            _player = new StreamMp3Player();
                            break;
                        default:
                            throw new Exception("Unhandled StreamSourceType:  AudioController.cs");
                    }
                }

                _nowPlaying.CurrentTime = TimeSpan.Zero;

                // START
                _player.Play(_nowPlaying.Source, _nowPlaying.SourceType, _nowPlaying.Bitrate, _nowPlaying.Codec);

                if (this.PlaybackStartedEvent != null)
                    this.PlaybackStartedEvent(_nowPlaying);
            }
        }
        private void StopImpl()
        {
            if (_player == null)
                return;

            if (_player.GetPlaybackState() == PlaybackState.Playing ||
                _player.GetPlaybackState() == PlaybackState.Paused)
            {
                _player.Stop();
                _player.Dispose();
                _player.PlaybackStoppedEvent -= OnPlaybackStopped;
                _player.PlaybackTickEvent -= OnPlaybackTick;
                _player = null;
            }
        }
        private void OnPlaybackTick(TimeSpan currentTime)
        {
            // No real life-cycle control here.. View -> Controller -> Timer (dispose)
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_nowPlaying == null)
                    return;

                _nowPlaying.CurrentTime = currentTime;

            }, DispatcherPriority.Background);
        }

        private void OnPlaybackStopped()
        {
            if (this.PlaybackStoppedEvent != null)
                this.PlaybackStoppedEvent(_nowPlaying);
        }
    }
}
