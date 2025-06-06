namespace AudioStation.Core.Component
{
    public enum LibraryLoadType
    {
        Mp3File,
        M3UFile
    }
    public class LibraryLoaderWorkItem
    {
        public string FileName { get; set; }
        public LibraryLoadType LoadType { get; set; }
        public bool ProcessingSuccessful { get; set; }
        public bool ProcessingComplete { get; set; }

        public LibraryLoaderWorkItem(string fileName, LibraryLoadType loadType)
        {
            this.FileName = fileName;
            this.LoadType = loadType;
            this.ProcessingSuccessful = false;
            this.ProcessingComplete = false;
        }
        public LibraryLoaderWorkItem(LibraryLoaderWorkItem copy)
        {
            this.FileName = copy.FileName;
            this.LoadType = copy.LoadType;
            this.ProcessingSuccessful = copy.ProcessingSuccessful;
            this.ProcessingComplete = copy.ProcessingComplete;
        }
    }
}
