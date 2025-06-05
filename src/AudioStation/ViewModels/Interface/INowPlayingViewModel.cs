using AudioStation.Model;

namespace AudioStation.ViewModels.Interface
{
    public interface INowPlayingViewModel
    {
        /// <summary>
        /// Source of the audio data:  file, network, etc...
        /// </summary>
        string Source { get; set; }
        StreamSourceType SourceType { get; set; }

        string Artist { get; set; }
        string Album { get; set; }
        string Title { get; set; }
        TimeSpan CurrentTime { get; set; }
        TimeSpan Duration { get; set; }
    }
}
