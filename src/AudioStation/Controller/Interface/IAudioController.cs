using AudioStation.Event;

using NAudio.Wave;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController
    {
        event SimpleEventHandler PlaybackStoppedEvent;

        void Play(string fileName);
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
