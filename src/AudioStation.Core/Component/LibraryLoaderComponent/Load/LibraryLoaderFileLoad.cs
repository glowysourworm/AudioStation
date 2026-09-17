using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderFileLoad : ILibraryLoaderLoad
    {
        public int OwnerId { get; private set; }
        public LibraryLoadType Type { get; private set; }
        public string File { get; private set; }

        public LibraryLoaderFileLoad(int ownerId, LibraryLoadType loadType, string file)
        {
            this.OwnerId = ownerId;
            this.File = file;
            this.Type = loadType;
        }
    }
}
