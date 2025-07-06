namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput
{
    public class LibraryLoaderImportLoadOutput : LibraryLoaderOutputBase
    {
        public List<LibraryLoaderImportFileResult> Results { get; set; }

        public LibraryLoaderImportLoadOutput()
        {
            this.Results = new List<LibraryLoaderImportFileResult>();
        }
    }
}
