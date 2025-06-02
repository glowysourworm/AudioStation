using System;
using System.Collections.Generic;
using System.Linq;

using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModel.Comparer;

using NAudio.Wave;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Model
{
    public class Playlist : ViewModelBase
    {
        string _name;
        TitleViewModel _nowPlaying;
        TimeSpan _nowPlayingCurrentTime;
        PlaybackState _nowPlayingState;
        SortedObservableCollection<TitleViewModel> _tracks;

        // Actual track queue: needed to work with playback
        List<TitleViewModel> _queue;
        int _queueCursor;

        public string Name
        {
            get { return _name; }
            set { this.SetProperty(ref _name, value); }
        }
        public TitleViewModel NowPlaying
        {
            get { return _nowPlaying; }
            set { this.SetProperty(ref _nowPlaying, value); }
        }
        public TimeSpan NowPlayingCurrentTime
        {
            get { return _nowPlayingCurrentTime; }
        }
        public SortedObservableCollection<TitleViewModel> Tracks
        {
            get { return _tracks; }
            set { this.SetProperty(ref _tracks, value); }
        }
        public PlaybackState NowPlayingState
        {
            get { return _nowPlayingState; }
            set { this.SetProperty(ref _nowPlayingState, value); }
        }


        public Playlist()
        {
            this.Name = string.Empty;
            this.Tracks = new SortedObservableCollection<TitleViewModel>(new TitleViewModelDefaultComparer());

            _queue = new List<TitleViewModel>();
            _queueCursor = -1;
        }

        public bool ContainsTrack(TitleViewModel track)
        {
            return this.Tracks.Any(x => x.FileName == track.FileName);
        }

        public void LoadPlayback(TitleViewModel? selectedTrack)
        {
            var trackIndex = selectedTrack == null ? 0 : this.Tracks.IndexOf(selectedTrack);

            // Selected Track: Load that track and everything after
            //
            _queue.AddRange(this.Tracks.Skip(trackIndex).Take(this.Tracks.Count - trackIndex));
            _queueCursor = 0;

            LoadCurrentPlayback();
        }
        public int PlaybackCount()
        {
            return _queue.Count;
        }
        public void PlaybackNext()
        {
            if (_queue.Count == 0)
                throw new ArgumentException("Must load playlist before using playback functions");

            _queueCursor = _queueCursor + 1 < _queue.Count ? _queueCursor + 1 : _queueCursor;

            LoadCurrentPlayback();
        }
        public void PlaybackLast()
        {
            if (_queue.Count == 0)
                throw new ArgumentException("Must load playlist before using playback functions");

            _queueCursor = _queueCursor - 1 >= 0 ? _queueCursor - 1 : _queueCursor;

            LoadCurrentPlayback();
        }
        public void PlaybackFirst()
        {
            if (_queue.Count == 0)
                throw new ArgumentException("Must load playlist before using playback functions");

            _queueCursor = _queue.Count > 0 ? 0 : -1;

            LoadCurrentPlayback();
        }
        public bool PlaybackHasLast()
        {
            return _queueCursor - 1 >= 0;
        }
        public bool PlaybackHasNext()
        {
            return _queueCursor + 1 < _queue.Count;
        }
        public void UnloadPlayback()
        {
            _queue.Clear();
            _queueCursor = -1;

            UpdateCurrentTrackTime(TimeSpan.Zero);
        }
        public void UpdateCurrentTrackTime(TimeSpan currentTime)
        {
            _nowPlayingCurrentTime = currentTime;

            OnPropertyChanged("NowPlayingCurrentTime");
        }

        private void LoadCurrentPlayback()
        {
            if (_queue.Count == 0)
                throw new ArgumentException("Must load playlist before using playback functions");

            if (_queueCursor >= 0 && _queueCursor < _queue.Count)
            {
                this.NowPlaying = _queue[_queueCursor];

                UpdateCurrentTrackTime(TimeSpan.Zero);
            }
        }
    }
}
