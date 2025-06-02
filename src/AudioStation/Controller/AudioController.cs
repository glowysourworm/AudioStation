using System;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;

using Avalonia.Threading;

using NAudio.Wave;

namespace AudioStation.Controller
{
    public class AudioController : IAudioController
    {
        public event SimpleEventHandler<Playlist, TitleViewModel> PlaybackStartedEvent;
        public event SimpleEventHandler<Playlist> PlaybackStoppedEvent;

        private SimpleMp3Player _player;
        private Playlist? _playlist;

        private bool _userStopIssued;

        public AudioController()
        {
            _player = new SimpleMp3Player();
            _playlist = null;
            _player.PlaybackStoppedEvent += OnPlaybackStopped;
            _player.PlaybackTickEvent += OnPlaybackTick;

            _userStopIssued = false;
        }

        ~AudioController()
        {
            _player.Dispose();
            _player = null;
        }

        public PlaybackState GetPlaybackState()
        {
            return _player.GetPlaybackState();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void Play(Playlist playlist)
        {
            if (playlist.PlaybackCount() == 0)
                throw new ArgumentException("Cannot play an empty playlist:  AudioController.cs");

            StopImpl();

            // Set Playlist
            _playlist = playlist;
            _playlist.PlaybackFirst();

            StartImpl();
        }

        public void Stop()
        {
            _userStopIssued = true;

            StopImpl();
        }

        public void SetTrack(TitleViewModel titleViewModel)
        {
            if (_playlist == null)
                throw new Exception("Trying to use AudioController without first loading playlist");

            if (!_playlist.ContainsTrack(titleViewModel))
                throw new ArgumentException(string.Format("Playlist does not contain the requested track:  {0}", titleViewModel.FileName));

            StopImpl();

            _playlist.LoadPlayback(titleViewModel);

            StartImpl();
        }

        public void NextTrack()
        {
            if (_playlist == null)
                throw new Exception("Trying to use AudioController without first loading playlist");

            StopImpl();

            // Ignore
            if (!_playlist.PlaybackHasNext())
                return;
            else
                _playlist.PlaybackNext();

            StartImpl();
        }

        public void PreviousTrack()
        {
            if (_playlist == null)
                throw new Exception("Trying to use AudioController without first loading playlist");

            StopImpl();

            // Rewind -> Stop
            if (!_playlist.PlaybackHasLast())
            {
                _playlist.PlaybackFirst();
            }
            else
                _playlist.PlaybackLast();

            StartImpl();
        }

        private void StartImpl()
        {
            if (_player.GetPlaybackState() == PlaybackState.Playing ||
                _player.GetPlaybackState() == PlaybackState.Paused)
            {
                return;
            }

            if (_playlist != null)
            {
                _playlist.NowPlayingState = PlaybackState.Playing;
                _playlist.UpdateCurrentTrackTime(TimeSpan.Zero);

                if (this.PlaybackStartedEvent != null)
                    this.PlaybackStartedEvent(_playlist, _playlist.NowPlaying);
            }
        }
        private void StopImpl()
        {
            if (_player.GetPlaybackState() == PlaybackState.Playing ||
                _player.GetPlaybackState() == PlaybackState.Paused)
            {
                _player.Stop();
            }

            if (_playlist != null)
            {
                _playlist.NowPlayingState = PlaybackState.Stopped;
                _playlist.UpdateCurrentTrackTime(TimeSpan.Zero);

                if (this.PlaybackStoppedEvent != null)
                    this.PlaybackStoppedEvent(_playlist);
            }
        }
        private void OnPlaybackTick(TimeSpan currentTime)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_playlist == null)
                    return;

                _playlist.UpdateCurrentTrackTime(currentTime);

                //if (this.PlaybackTickEvent != null)
                //    this.PlaybackTickEvent(_playlist.Tracks[_playlist.NowPlayingIndex], currentTime);

            }, DispatcherPriority.Background);
        }

        private void OnPlaybackStopped()
        {
            if (_playlist == null)
                return;

            if (_userStopIssued || _playlist.PlaybackCount() == 0)
            {
                _playlist.UnloadPlayback();

                StopImpl();
            }
            else if (!_userStopIssued && _playlist.PlaybackCount() > 0)
            {
                _playlist.PlaybackNext();

                StartImpl();
            }

            _userStopIssued = false;
        }
    }
}
