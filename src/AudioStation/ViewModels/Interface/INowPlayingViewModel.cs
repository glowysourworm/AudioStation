using AudioStation.Core.Model;

namespace AudioStation.ViewModels.Interface
{
    public interface INowPlayingViewModel
    {
        /// <summary>
        /// Source of the audio data:  file, network, etc...
        /// </summary>
        string Source { get; set; }
        StreamSourceType SourceType { get; set; }

        // We need our own "codec" details for handling each of the audio libraries at runtime
        int Bitrate { get; set; }
        string Codec { get; set; }


        string Artist { get; set; }
        string Album { get; set; }
        string Title { get; set; }
        TimeSpan CurrentTime { get; set; }
        TimeSpan Duration { get; set; }
    }
}
