﻿using System.Windows.Media;

using AudioStation.Component.AudioProcessing;
using AudioStation.Component.AudioProcessing.Interface;
using AudioStation.Core.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component.AudioProcessing
{
    public class StreamMp3Player : IAudioPlayer
    {
        MediaPlayer _player;

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<StoppedEventArgs> PlaybackStoppedEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;        // Not going to use for streams
        public event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;

        public bool HasAudio
        {
            get { return _player?.HasAudio ?? false; }
        }

        public StreamMp3Player()
        {
            _player = new MediaPlayer();
            _player.MediaEnded += OnMediaEnded;
        }

        public void SetVolume(float volume)
        {
            _player.Volume = volume;
        }
        public void SetEqualizerGain(float frequency, float gain)
        {

        }
        public float GetVolume()
        {
            return (float)_player.Volume;
        }
        public void SetPosition(TimeSpan position)
        {
            _player.Position = position;
        }
        public void Play(string source, StreamSourceType type)
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
        public void Resume()
        {
            if (_player == null)
                throw new Exception("Trying to resume stopped playback (must re-start IAudioPlayer):  StreamMp3Player");

            if (!_player.HasAudio)
                throw new Exception("Trying to resume stopped playback (must re-start IAudioPlayer):  StreamMp3Player");

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
                this.PlaybackStoppedEvent(new StoppedEventArgs(null));
        }

        public void Dispose()
        {

        }
    }
}
