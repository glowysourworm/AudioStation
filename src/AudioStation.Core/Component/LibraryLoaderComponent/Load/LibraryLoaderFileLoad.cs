using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderFileLoad : ILibraryLoaderLoad
    {
        public LibraryLoadType Type { get; private set; }
        public string File { get; private set; }

        public LibraryLoaderFileLoad(LibraryLoadType loadType, string file)
        {
            this.File = file;
            this.Type = loadType;
        }
    }
}
