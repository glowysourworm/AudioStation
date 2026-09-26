using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output
{
    /// <summary>
    /// Contains details about an audio file's codec, duration, and pertinent audio information.
    /// </summary>
    public class LibraryLoaderAudioEncodingOutputPayload
    {
        public TimeSpan Duration { get; set; }
        public AudioEncoderInfo CodecInfo { get; set; }

        public LibraryLoaderAudioEncodingOutputPayload()
        {
            this.CodecInfo = new AudioEncoderInfo();
            this.Duration = TimeSpan.Zero;
        }
    }
}
