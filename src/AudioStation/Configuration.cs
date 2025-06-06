namespace AudioStation
{
    public class Configuration
    {
        public const string LIBRARY_DATABASE_FILE = ".AudioPlayerLibrary";

        public string DirectoryBase { get; set; }
        public string LibraryDatabaseFile { get; set; }

        public Configuration()
        {
            this.LibraryDatabaseFile = LIBRARY_DATABASE_FILE;
        }
    }
}
