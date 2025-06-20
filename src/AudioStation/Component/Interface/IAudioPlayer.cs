using AudioStation.Core.Model;

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
        float GetVolume();
        void Play(string source, StreamSourceType sourceType, int bitrate, string codec);
        void Pause();
        void Stop();

        PlaybackState GetPlaybackState();
    }
}
