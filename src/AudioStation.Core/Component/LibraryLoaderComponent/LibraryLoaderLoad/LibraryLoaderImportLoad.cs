namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderImportLoad : LibraryLoaderLoadBase
    {
        public LibraryLoaderImportLoad(string sourceFile, string destinationBaseDirectory, bool moveNotCopy) 
        {
            this.SourcePath = sourceFile;
            this.DestinationBaseDirectory = destinationBaseDirectory;
            this.DeleteSourceFile = moveNotCopy;
        }

        /// <summary>
        /// Mp3 file we're importing (FULL PATH)
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Destination (base directory) for the mp3 file. This will include the user's preference
        /// based on whether it is a music / audio-book file.
        /// </summary>
        public string DestinationBaseDirectory { get; private set; }

        /// <summary>
        /// Instruct loader to delete the source file when finished (move, instead of copy)
        /// </summary>
        public bool DeleteSourceFile { get; private set; }

        /// <summary>
        /// Flag for success of the import for this file
        /// </summary>
        public bool Success { get; set; }

        public override int GetFailureCount()
        {
            return this.Success ? 0 : 1;
        }

        public override double GetProgress()
        {
            return 0;
        }

        public override int GetSuccessCount()
        {
            return 0;
        }

        public override bool HasErrors()
        {
            return !this.Success;
        }
    }
}
