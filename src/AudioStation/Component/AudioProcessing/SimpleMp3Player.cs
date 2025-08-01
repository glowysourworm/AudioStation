﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using AudioStation.Component.AudioProcessing;
using AudioStation.Component.AudioProcessing.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component.AudioProcessing
{
    /// <summary>
    /// NAudio based Mp3 Player (see their docs)
    /// </summary>
    public class SimpleMp3Player : IAudioPlayer
    {
        const int CURRENT_TIME_UPDATE_MILLISECONDS = 10;

        Timer _timer;
        MediaPlayer _player;
        object _lock = new object();

        public event SimpleEventHandler<string> MessageEvent;
        public event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        public event SimpleEventHandler<StoppedEventArgs> PlaybackStoppedEvent;
        public event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;

        public bool HasAudio
        {
            get { return _player?.HasAudio ?? false; }
        }

        public SimpleMp3Player()
        {
            _player = new MediaPlayer();
            _player.MediaOpened += OnMediaOpened;
            _player.MediaEnded += OnMediaEnded;

            _timer = new Timer(OnTimerTick);
            _timer.Change(0, Timeout.Infinite);
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            if (this.PlaybackStoppedEvent != null)
                this.PlaybackStoppedEvent(new StoppedEventArgs(null));
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {

        }

        private void OnTimerTick(object? state)
        {
            if (Application.Current == null)
                return;

            ApplicationHelpers.BeginInvokeDispatcher(() =>
            {
                if (this.PlaybackTickEvent != null)
                    this.PlaybackTickEvent(_player.Position);

            }, DispatcherPriority.Background);
        }

        public void SetVolume(float volume)
        {
            _player.Volume = Math.Clamp(volume, 0, 1);
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
        public void Play(string fileName, StreamSourceType sourceType)
        {
            if (sourceType != StreamSourceType.File)
                throw new ArgumentException("Improper use of StreamMp3Player. Expecting file stream type");

            _timer.Change(0, CURRENT_TIME_UPDATE_MILLISECONDS);
            _player.Open(new Uri(fileName));
            _player.Play();
        }
        public void Resume()
        {
            if (!_player.HasAudio)
                throw new Exception("Trying to resume player in a stopped state:  SimpleMp3Player.cs");

            _timer.Change(0, CURRENT_TIME_UPDATE_MILLISECONDS);
            _player.Play();
        }
        public void Pause()
        {
            _timer.Change(0, Timeout.Infinite);
            _player.Pause();
        }

        public void Stop()
        {
            _timer.Change(0, Timeout.Infinite);
            _player.Stop();
        }

        public PlaybackState GetPlaybackState()
        {
            return PlaybackState.Playing;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _player.Stop();
        }
    }
}
