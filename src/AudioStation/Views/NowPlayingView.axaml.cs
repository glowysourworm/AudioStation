using System.Collections.Generic;
using System.Linq;

using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels;

using Avalonia.Controls;

namespace AudioStation;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView()
    {
        InitializeComponent();

        // Dependency Injection Needed:  too many other issues right now
        MainViewModel.AudioController.PlaybackStartedEvent += OnAudioControllerPlaybackStarted;
        MainViewModel.AudioController.PlaybackStoppedEvent += OnAudioControllerPlaybackStopped;
        MainViewModel.AudioController.TrackChangedEvent += OnAudioControllerTrackChanged;
    }
    ~NowPlayingView()
    {
        // Dependency Injection Needed:  too many other issues right now
        MainViewModel.AudioController.PlaybackStartedEvent -= OnAudioControllerPlaybackStarted;
        MainViewModel.AudioController.PlaybackStoppedEvent -= OnAudioControllerPlaybackStopped;
        MainViewModel.AudioController.TrackChangedEvent -= OnAudioControllerTrackChanged;
    }

    private void OnArtistSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var library = this.DataContext as Library;

        if (e.AddedItems.Count > 0 && library != null)
        {
            // View Artist Detail
            this.ArtistDetailLB.ItemsSource = (e.AddedItems[0] as ArtistViewModel).Albums;
        }
    }

    private void OnArtistDetailDoubleClick(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selectedTrack = (e.Source as Control).DataContext as TitleViewModel;
        var albums = this.ArtistDetailLB.ItemsSource as IEnumerable<AlbumViewModel>;

        if (selectedTrack != null && albums != null)
        {
            // Contains (by reference) not yet hooked up for sorted observable collection
            var selectedAlbum = albums.First(album => album.Tracks.Any(x => x.FileName == selectedTrack.FileName));

            LoadPlaylist(selectedTrack, selectedAlbum);
        }
    }

    private void OnPlaylistDoubleClick(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selectedTrack = (e.Source as Control).DataContext as TitleViewModel;
        var albums = this.ArtistDetailLB.ItemsSource as IEnumerable<AlbumViewModel>;

        if (selectedTrack != null && albums != null)
        {
            // Contains (by reference) not yet hooked up for sorted observable collection
            var selectedAlbum = albums.First(album => album.Tracks.Any(x => x.FileName == selectedTrack.FileName));

            LoadPlaylist(selectedTrack, selectedAlbum);
        }
    }

    private void LoadPlaylist(TitleViewModel selectedTitle, AlbumViewModel selectedAlbum)
    {
        var playlist = new Playlist();
        playlist.Name = selectedAlbum.Artist + " / " + selectedAlbum.Album;

        var index = 0;

        foreach (var track in selectedAlbum.Tracks)
        {
            playlist.Tracks.Add(track);

            if (track == selectedTitle)
                playlist.NowPlayingIndex = index;

            index++;
        }

        // Set View Model
        this.PlaylistContainer.DataContext = playlist;

        // Play Selected Track
        MainViewModel.AudioController.Play(playlist);
    }

    private void SetNowPlaying(Playlist playlist, bool forceStop)
    {
        for (int index = 0; index < playlist.Tracks.Count; index++)
        {
            playlist.Tracks[index].NowPlaying = (index == playlist.NowPlayingIndex) && !forceStop;
        }
    }

    private void OnAudioControllerPlaybackStarted(Playlist playlist)
    {
        SetNowPlaying(playlist, false);
    }

    private void OnAudioControllerPlaybackStopped(Playlist playlist)
    {
        SetNowPlaying(playlist, true);
    }

    private void OnAudioControllerTrackChanged(Playlist playlist)
    {
        SetNowPlaying(playlist, false);
    }
}