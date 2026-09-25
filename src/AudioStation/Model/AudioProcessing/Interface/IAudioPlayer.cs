using AudioStation.Core.Model;

using CSCore;
using CSCore.SoundOut;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Model.AudioProcessing.Interface
{
    public interface IAudioPlayer : IDisposable
    {
        event SimpleEventHandler<string> MessageEvent;
        event SimpleEventHandler<TimeSpan> PlaybackTickEvent;
        event SimpleEventHandler<EqualizerResultSet> EqualizerCalculated;
        event SimpleEventHandler<StoppedEventArgs> PlaybackStoppedEvent;

        void SetVolume(float volume);
        float GetVolume();
        void SetPosition(TimeSpan position);
        void SetPosition(float positionRatio);
        void SetEqualizerGain(float frequency, float gain);
        void Play(string source, StreamSourceType sourceType);
        void Resume();
        void Pause();
        void Stop();

        TimeSpan GetDuration();

        bool HasAudio { get; }

        PlaybackState GetPlaybackState();
    }
}
