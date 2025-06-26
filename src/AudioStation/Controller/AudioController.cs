using System.Windows;
using System.Windows.Threading;

using AudioStation.Component;
using AudioStation.Component.AudioProcessing;
using AudioStation.Component.AudioProcessing.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Model;
using AudioStation.Event;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioController))]
    public class AudioController : IAudioController
    {
        public event SimpleEventHandler<TimeSpan> CurrentTimeUpdated;
        public event SimpleEventHandler<EqualizerResultSet> CurrentBandLevelsUpdated;

        private readonly IIocEventAggregator _eventAggregator;
        private IAudioPlayer _player;
        private string _streamSource;
        private StreamSourceType _streamSourceType;
        private bool _userStopped;

        [IocImportingConstructor]
        public AudioController(IIocEventAggregator eventAggregator)
        {
            _player = null;
            _eventAggregator = eventAggregator;
            _streamSource = null;
            _streamSourceType = StreamSourceType.Network;
            _userStopped = false;

            eventAggregator.GetEvent<LoadPlaybackEvent>().Subscribe(eventData =>
            {
                Stop();

                Load(eventData.Source, eventData.SourceType);
            });
            eventAggregator.GetEvent<StartPlaybackEvent>().Subscribe(() =>
            {
                if (_player != null && _player.GetPlaybackState() == PlaybackState.Paused)
                    Resume();

                else if (_streamSource != null)
                {
                    Play();
                }                    

                else
                    throw new Exception("Trying to start playback before loading media:  IAudioController");
            });
            eventAggregator.GetEvent<StopPlaybackEvent>().Subscribe(() =>
            {
                _userStopped = true;

                if (_player != null)        // Race condition on NAudio (we need this handled)
                    Stop();

                _userStopped = false;
            });
            eventAggregator.GetEvent<PausePlaybackEvent>().Subscribe(() =>
            {
                _userStopped = true;

                if (_player != null)
                    Pause();

                _userStopped = false;
            });
            eventAggregator.GetEvent<UpdateVolumeEvent>().Subscribe(volume =>
            {
                _player?.SetVolume((float)volume);
            });
            eventAggregator.GetEvent<UpdateEqualizerGainEvent>().Subscribe(eventData =>
            {
                _player?.SetEqualizerGain(eventData.Frequency, eventData.Gain);
            });
            eventAggregator.GetEvent<PlaybackPositionChangedEvent>().Subscribe(position =>
            {
                _player?.SetPosition(position);
            });
        }

        ~AudioController()
        {
            if (_player != null)
            {
                _player.Dispose();
                _player.PlaybackStoppedEvent -= OnPlaybackStopped;
                _player.PlaybackTickEvent -= OnPlaybackTick;
                _player.EqualizerCalculated -= OnEqualizerCalculated;
                _player = null;
                _streamSource = null;
            }
        }

        public PlaybackState GetPlaybackState()
        {
            return _player.GetPlaybackState();
        }

        public void Pause()
        {
            _player.Pause();

            _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(new PlaybackStateChangedEventData()
            { 
                State = PlayStopPause.Pause
            });
        }

        public void Load(string streamSource, StreamSourceType streamSourceType)
        {
            _streamSource = streamSource;
            _streamSourceType = streamSourceType;
        }

        public void Play()
        {
            if (_player != null &&
               (_player.GetPlaybackState() == PlaybackState.Paused ||
                _player.GetPlaybackState() == PlaybackState.Playing))
                _player.Resume();

            else if (!string.IsNullOrEmpty(_streamSource))
            {
                if (_player != null)
                {
                    _player.Dispose();
                    _player = null;
                }

                switch (_streamSourceType)
                {
                    case StreamSourceType.File:
                        _player = new SimpleMp3PlayerWithEqualizer();
                        break;
                    case StreamSourceType.Network:
                        _player = new StreamMp3Player();
                        break;
                    default:
                        throw new Exception("Unhandled StreamSourceType:  AudioController.cs");
                }

                // Zero-Time
                OnPlaybackTick(TimeSpan.Zero);

                // Hook Events
                _player.SetVolume(1);
                _player.PlaybackStoppedEvent += OnPlaybackStopped;
                _player.PlaybackTickEvent += OnPlaybackTick;
                _player.EqualizerCalculated += OnEqualizerCalculated;

                // START
                _player.Play(_streamSource, _streamSourceType);

                // New Player -> get volume (device) setting
                _eventAggregator.GetEvent<PlaybackVolumeUpdatedEvent>().Publish(_player.GetVolume());
                _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(new PlaybackStateChangedEventData()
                {
                    State = PlayStopPause.Play
                });
            }
        }

        private void Resume()
        {
            _player.Resume();

            _eventAggregator.GetEvent<PlaybackStateChangedEvent>().Publish(new PlaybackStateChangedEventData()
            { 
                State = PlayStopPause.Play
            });
        }

        public void Stop()
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
                _player.EqualizerCalculated -= OnEqualizerCalculated;
                _player = null;
            }

            // Manually update current time to zero
            if (this.CurrentTimeUpdated != null)
                this.CurrentTimeUpdated(TimeSpan.Zero);
        }

        private void OnPlaybackTick(TimeSpan currentTime)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.Invoke(OnPlaybackTick, DispatcherPriority.Background, currentTime);
            else
            {
                if (this.CurrentTimeUpdated != null)
                    this.CurrentTimeUpdated(currentTime);
            }
        }

        private void OnEqualizerCalculated(EqualizerResultSet resultSet)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.Invoke(OnEqualizerCalculated, DispatcherPriority.Background, resultSet);
            else
            {
                if (this.CurrentBandLevelsUpdated != null)
                    this.CurrentBandLevelsUpdated(resultSet);
            }
        }

        private void OnPlaybackStopped(StoppedEventArgs eventArgs)
        {
            _eventAggregator.GetEvent<PlaybackStateChangedEvent>()
                            .Publish(new PlaybackStateChangedEventData()
                            {
                                State = PlayStopPause.Stop,
                                EndOfTrack = eventArgs.Exception == null && !_userStopped
                            });
        }

        public void Dispose()
        {
            if (_player != null)
            {
                _player.Dispose();
                _player = null;
            }

            _streamSource = null;
        }
    }
}
