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

        // BINDING NOT WORKING!
        this.DataContextChanged += NowPlayingView_DataContextChanged;
        this.Loaded += NowPlayingView_Loaded;
    }

    private void NowPlayingView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;

        if (viewModel != null)
        {
            this.ArtistLB.ItemsSource = viewModel.Library.ValidArtists;
        }
    }

    private void NowPlayingView_DataContextChanged(object? sender, System.EventArgs e)
    {
        var viewModel = this.DataContext as MainViewModel;

        if (viewModel != null)
        {
            this.ArtistLB.ItemsSource = viewModel.Library.ValidArtists;
        }
    }

    private void OnArtistSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        var library = (this.DataContext as MainViewModel)?.Library;

        if (e.AddedItems.Count > 0 && library != null)
        {
            // ??? (it was null...)
            if (e.AddedItems[0] != null)
            {
                // View Artist Detail
                this.ArtistDetailLB.ItemsSource = (e.AddedItems[0] as ArtistViewModel).Albums;
            }
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

        // Load tracks for playback
        foreach (var track in selectedAlbum.Tracks)
        {
            playlist.Tracks.Add(track);
        }

        // Setup playback (needs revision; but this works with the IAudioController)
        playlist.LoadPlayback(selectedTitle);

        // Set View Model
        (this.DataContext as MainViewModel).Playlist = playlist;

        // Play Selected Track(s) (the audio controller <-> playlist handle the rest)
        MainViewModel.AudioController.Play(playlist);
    }
}