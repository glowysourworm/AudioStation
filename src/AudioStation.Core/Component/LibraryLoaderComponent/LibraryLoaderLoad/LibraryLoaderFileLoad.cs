using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderFileLoad : LibraryLoaderLoadBase
    {
        public string File { get; private set; }

        public LibraryLoaderFileLoad(string file)
        {
            this.File = file;
        }
    }
}
