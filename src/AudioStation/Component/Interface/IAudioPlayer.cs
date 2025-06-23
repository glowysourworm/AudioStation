using AudioStation.Component.AudioProcessing;
using AudioStation.Core.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Component.Interface
{
    public interface IAudioPlayer : IDisposable
    {
        event SimpleEventHandler<string> MessageEvent;
        event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;
        event SimpleEventHandler PlaybackStoppedEvent;

        void SetVolume(float volume);
        float GetVolume();
        void SetPosition(TimeSpan position);
        void SetEqualizerGain(float frequency, float gain);
        void Play(string source, StreamSourceType sourceType);
        void Resume();
        void Pause();
        void Stop();

        bool HasAudio { get; }

        PlaybackState GetPlaybackState();
    }
}
