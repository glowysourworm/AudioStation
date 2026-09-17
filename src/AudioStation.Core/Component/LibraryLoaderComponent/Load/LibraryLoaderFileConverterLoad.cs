using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderFileConverterLoad : ILibraryLoaderLoad
    {
        public LibraryLoadType Type { get; private set; }
        public string FileIn { get; set; }
        public string FileOut { get; set; }
        public AudioEncoderInfo EncoderInfo { get; set; }

        public LibraryLoaderFileConverterLoad(LibraryLoadType loadType)
        {
            this.Type = loadType;
            this.FileIn = string.Empty;
            this.FileOut = string.Empty;
        }
    }
}
