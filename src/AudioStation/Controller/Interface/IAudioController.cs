using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Model.AudioProcessing;

using CSCore.SoundOut;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    public interface IAudioController : IAudioStationDataService, IDisposable
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
        void SetCurrentTime(TimeSpan time);
        PlaybackState GetPlaybackState();
    }
}
