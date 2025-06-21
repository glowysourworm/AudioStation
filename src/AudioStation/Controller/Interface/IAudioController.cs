using AudioStation.ViewModels.Interface;

using NAudio.Wave;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController : IDisposable
    {
        void Play(INowPlayingViewModel nowPlaying);
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
