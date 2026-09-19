namespace AudioStation.Core.Component.LibraryLoaderComponent.Payload.Input
{
    public class LibraryLoaderFilePayload
    {
        public string File { get; private set; }

        public LibraryLoaderFilePayload(string file)
        {
            this.File = file;
        }
    }
}
