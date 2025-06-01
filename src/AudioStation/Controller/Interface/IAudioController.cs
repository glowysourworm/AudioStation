using AudioStation.Event;
using AudioStation.Model;

using NAudio.Wave;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController
    {
        event SimpleEventHandler<Playlist> PlaybackStartedEvent;
        event SimpleEventHandler<Playlist> PlaybackStoppedEvent;
        event SimpleEventHandler<Playlist> TrackChangedEvent;

        void Play(Playlist playlist);
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
