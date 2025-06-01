using System;

using AudioStation.Component;
using AudioStation.Controller.Interface;
using AudioStation.Event;
using AudioStation.Model;

using NAudio.Wave;

namespace AudioStation.Controller
{
    public class AudioController : IAudioController
    {
        public event SimpleEventHandler<Playlist> PlaybackStartedEvent;
        public event SimpleEventHandler<Playlist> PlaybackStoppedEvent;
        public event SimpleEventHandler<Playlist> TrackChangedEvent;

        private SimpleMp3Player _player;
        private Playlist? _playlist;

        public AudioController()
        {
            _player = new SimpleMp3Player();
            _playlist = null;
            _player.PlaybackStoppedEvent += OnPlaybackStopped;
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
            if (playlist.Tracks.Count == 0)
                throw new ArgumentException("Cannot play an empty playlist:  AudioController.cs");

            if (_player.GetPlaybackState() == PlaybackState.Playing ||
                _player.GetPlaybackState() == PlaybackState.Paused)
            {
                _player.Stop();

                // -> Resets Index
            }

            _playlist = playlist;

            ContinuePlaylist(false);

            if (this.PlaybackStartedEvent != null)
                this.PlaybackStartedEvent(_playlist);
        }

        public void Stop()
        {
            if (_playlist != null)
                _playlist.NowPlayingIndex = 0;

            _player.Stop();
        }

        private void ContinuePlaylist(bool increment)
        {
            if (_playlist == null)
                throw new ArgumentException("Playlist must be set before calling AudioController.ContinuePlaylist");

            if (_playlist.NowPlayingIndex < 0 ||
                _playlist.NowPlayingIndex >= _playlist.Tracks.Count)
            {
                _playlist.NowPlayingIndex = 0;
            }
            else if (increment)
            {
                _playlist.NowPlayingIndex++;
            }

            _player.Play(_playlist.Tracks[_playlist.NowPlayingIndex].FileName);

            if (this.TrackChangedEvent != null)
                this.TrackChangedEvent(_playlist);
        }

        private void OnPlaybackStopped()
        {
            if (_playlist == null)
                return;

            // Cycle Playlist (need repeat-behavior selection first)
            if (_playlist.NowPlayingIndex == _playlist.Tracks.Count - 1)
            {
                if (this.PlaybackStoppedEvent != null)
                    this.PlaybackStoppedEvent(_playlist);
            }
            else
            {
                // -> Play "NowPlaying" track
                ContinuePlaylist(true);
            }
        }
    }
}
