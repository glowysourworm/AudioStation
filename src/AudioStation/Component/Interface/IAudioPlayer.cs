using AudioStation.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component.Interface
{
    public interface IAudioPlayer : IDisposable
    {
        event SimpleEventHandler<string> MessageEvent;
        event SimpleEventHandler<TimeSpan> PlaybackTickEvent;

        event SimpleEventHandler PlaybackStoppedEvent;

        void SetVolume(float volume);
        void Play(string source, StreamSourceType sourceType);
        void Pause();
        void Stop();

        PlaybackState GetPlaybackState();
    }
}
