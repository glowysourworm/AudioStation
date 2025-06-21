using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Model;
using AudioStation.Event;
using AudioStation.ViewModels.Interface;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioController))]
    public class AudioController : IAudioController
    {
        private readonly IIocEventAggregator _eventAggregator;
        private IAudioPlayer _player;
        private INowPlayingViewModel _nowPlaying;

        [IocImportingConstructor]
        public AudioController(IIocEventAggregator eventAggregator)
        {
            _player = null;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<LoadPlaybackEvent>().Subscribe(nowPlaying =>
            {
                Stop();

                _nowPlaying = nowPlaying;
            });
            eventAggregator.GetEvent<StartPlaybackEvent>().Subscribe(() =>
            {
                if (_player != null && _player.HasAudio)
                    Resume();

                else if (_nowPlaying != null)
                    Play(_nowPlaying);

                else
                    throw new Exception("Trying to start playback before loading media:  IAudioController");
            });
            eventAggregator.GetEvent<StopPlaybackEvent>().Subscribe(() =>
            {
                if (_player != null)
                    Stop();
            });
            eventAggregator.GetEvent<PausePlaybackEvent>().Subscribe(() =>
            {
                if (_player != null)
                    Pause();
            });
            eventAggregator.GetEvent<UpdateVolumeEvent>().Subscribe(volume =>
            {
                if (_player != null)
                    _player.SetVolume((float)volume);
            });
            eventAggregator.GetEvent<PlaybackPositionChangedEvent>().Subscribe(position =>
            {
                if (_player != null)
                    _player.SetPosition(position);
            });
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

            _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(PlayStopPause.Pause);
        }

        public void Play(INowPlayingViewModel nowPlaying)
        {
            StopImpl();

            _nowPlaying = nowPlaying;

            StartImpl();
        }

        private void Resume()
        {
            _player.Resume();

            _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(PlayStopPause.Play);
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
                        case StreamSourceType.File:
                            _player = new SimpleMp3Player();
                            break;
                        case StreamSourceType.Network:
                            _player = new StreamMp3Player();
                            break;
                        default:
                            throw new Exception("Unhandled StreamSourceType:  AudioController.cs");
                    }
                }

                _nowPlaying.CurrentTime = TimeSpan.Zero;

                // Hook Events
                _player.SetVolume(1);
                _player.PlaybackStoppedEvent += OnPlaybackStopped;
                _player.PlaybackTickEvent += OnPlaybackTick;

                // START
                _player.Play(_nowPlaying.Source, _nowPlaying.SourceType, _nowPlaying.Bitrate, _nowPlaying.Codec);

                // New Player -> get volume (device) setting
                _eventAggregator.GetEvent<PlaybackVolumeUpdatedEvent>().Publish(_player.GetVolume());
                _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(PlayStopPause.Play);
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
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.Invoke(OnPlaybackTick, DispatcherPriority.Background, currentTime);
            else
            {
                if (_nowPlaying == null)
                    return;

                _nowPlaying.CurrentTime = currentTime;
            }
        }

        private void OnPlaybackStopped()
        {
            _eventAggregator.GetEvent<PlaybackStateChangedEvent>()
                            .Publish(PlayStopPause.Stop);
        }

        public void Dispose()
        {
            if (_player != null)
            {
                _player.Dispose();
                _player = null;
            }
        }
    }
}
