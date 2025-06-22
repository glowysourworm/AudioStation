using AudioStation.Component.AudioProcessing;
using AudioStation.Core.Model;

using NAudio.Wave;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController : IDisposable
    {
        /// <summary>
        /// Event occurs when the stream's current time is updated
        /// </summary>
        public event SimpleEventHandler<TimeSpan> CurrentTimeUpdated;

        /// <summary>
        /// Event occurs when the stream's equalizer levels update
        /// </summary>
        public event SimpleEventHandler<EqualizerResultSet> CurrentBandLevelsUpdated;

        void Load(string streamSource, StreamSourceType streamSourceType);
        void Play();
        void Stop();
        void Pause();
        PlaybackState GetPlaybackState();
    }
}
