using AudioStation.ViewModels.Interface;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController : IDisposable
    {
        event SimpleEventHandler<INowPlayingViewModel> PlaybackStartedEvent;
        event SimpleEventHandler<INowPlayingViewModel> PlaybackStoppedEvent;

        void Play(INowPlayingViewModel nowPlaying);
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
