using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Model;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderFileConverterLoad : ILibraryLoaderLoad
    {
        public LibraryLoadType LoadType { get; private set; }
        public int OwnerId { get; private set; }
        public string FileIn { get; set; }
        public string FileOut { get; set; }
        public AudioEncoderInfo EncoderInfo { get; set; }

        public LibraryLoaderFileConverterLoad(int ownerId, LibraryLoadType loadType)
        {
            this.OwnerId = ownerId;
            this.LoadType = loadType;
            this.FileIn = string.Empty;
            this.FileOut = string.Empty;
        }
    }
}
