using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.DataComponent
{
    public class NowPlayingPlaylistViewModel : DataComponentViewModelBase
    {
        PlaylistViewModel? _playlist;
        PlaylistEntryViewModel? _currentTrack;

        SimpleCommand<PlaylistEntryViewModel> _setPlayingCommand;

        public PlaylistViewModel? Playlist
        {
            get { return _playlist; }
            set { this.RaiseAndSetIfChanged(ref _playlist, value); }
        }
        public PlaylistEntryViewModel? CurrentTrack
        {
            get { return _currentTrack; }
            private set { this.RaiseAndSetIfChanged(ref _currentTrack, value); }
        }

        public SimpleCommand<PlaylistEntryViewModel> SetPlayingCommand
        {
            get { return _setPlayingCommand; }
            set { this.RaiseAndSetIfChanged(ref _setPlayingCommand, value); }
        }

        public NowPlayingPlaylistViewModel(IIocEventAggregator eventAggregator) : base("Now Playing Playlist")
        {
            this.SetPlayingCommand = new SimpleCommand<PlaylistEntryViewModel>(entry =>
            {
                if (this.Playlist == null)
                    return;

                if (!this.Playlist.Entries.Contains(entry))
                    throw new ArgumentException("Invalid playlist entry. Must use instances that are currently in the playlist");

                this.CurrentTrack = entry;
            });
        }


        /// <summary>
        /// Sets "Playing" entry; but does not raise the command to start playback
        /// </summary>
        public void SetPlaying(PlaylistEntryViewModel entry)
        {
            if (this.Playlist == null)
                return;

            if (!this.Playlist.Entries.Contains(entry))
                throw new ArgumentException("Invalid playlist entry. Must use instances that are currently in the playlist");

            this.CurrentTrack = entry;
        }

        public override void Initialize(IAudioStationConfiguration configuration)
        {
        }

        public override void Dispose()
        {
            this.Playlist = null;
            this.CurrentTrack = null;
        }
    }
}
