using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input
{
    public class LibraryLoaderFileConverterPayload
    {
        public string FileIn { get; set; }
        public string FileOut { get; set; }
        public AudioEncoderInfo EncoderInfo { get; set; }

        public LibraryLoaderFileConverterPayload()
        {
            this.FileIn = string.Empty;
            this.FileOut = string.Empty;
        }
    }
}
