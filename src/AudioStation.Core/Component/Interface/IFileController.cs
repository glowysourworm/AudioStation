namespace AudioStation.Core.Component.Interface
{
    public interface IFileController
    {
        /// <summary>
        /// Removes un-friendly characters for the file path. Use for 
        /// directory names / file paths. The file name has a different illegal character set - so set 
        /// isFileName = true (iff) the final string is a file name. Otherwise, set it to false, and the 
        /// path will be processed according to what is "legal" for a directory (and, in this case, "friendly"). 
        /// Also, decodes UTF-8 escape sequences.
        /// </summary>
        string MakeFriendlyPath(bool isFileName, params string[] filePath);

        /// <summary>
        /// Creates a directory using standard .NET calls. Throws exceptions ordinarily.
        /// </summary>
        void CreateDirectory(string path);

        /// <summary>
        /// Checks for directory ordinarily - with logging and exceptions.
        /// </summary>
        bool DirectoryExists(string path);

        /// <summary>
        /// Checks for file ordinarily - with logging and exceptions.
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Moves file, checking parts of directory paths, checking folder permissions, and logging details. Returns true if 
        /// the file is moved successfully.
        /// </summary>
        void MigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false);

        /// <summary>
        /// Checks all possible file movement issues (that are allowed without yet moving the file); and returns the result. DOES NOT
        /// CREATE / DELETE ANY FILES OR FOLDERS.
        /// </summary>
        bool CanMigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false);
    }
}
