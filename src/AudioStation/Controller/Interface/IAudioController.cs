using System;

using AudioStation.Event;
using AudioStation.Model;
using AudioStation.ViewModel.LibraryViewModel;

using NAudio.Wave;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController
    {
        event SimpleEventHandler<Playlist, TitleViewModel> PlaybackStartedEvent;
        event SimpleEventHandler<Playlist> PlaybackStoppedEvent;

        void SetTrack(TitleViewModel titleViewModel);
        void NextTrack();
        void PreviousTrack();

        void Play(Playlist playlist);
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
