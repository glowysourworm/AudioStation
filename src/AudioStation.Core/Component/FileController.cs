using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IFileController))]
    public class FileController : IFileController
    {
        [IocImportingConstructor]
        public FileController()
        {

        }

        public bool CanMigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false)
        {
            try
            {
                // Procedure
                //
                // -2) Verify "Delete Source X" Permissions
                // -1) Verify that destination path is "friendly"
                // 0)  Verify that some base part of the path exists as a directory
                // 1)  Verify all directory parts up to the leaf (that exists)
                // 2)  Verify permissions on the leaf (both files / directories)
                //

                // Source "Delete X"
                //
                if ((deleteSource || deleteSourceEmptyFolder) &&
                   !HasPermissions(sourcePath, FileSystemRights.Delete))
                    return false;

                // Destination Friendly Path
                //
                if (!AreFriendlyPath(false, destinationPath))
                    return false;

                var directory = Path.GetDirectoryName(destinationPath);

                if (string.IsNullOrEmpty(directory))
                    return false;

                var directoryParts = CreateDirectoryPath(directory);
                var parentDirectoryExists = false;
                var parentDirectoryPermissions = false;

                // Verify Directory:  If Exists (Validate Permissions on the leaf)
                //
                for (int index = 0; index < directoryParts.Length; index++)
                {
                    // Some piece of the parent directory
                    var parentDirectory = directoryParts[index];

                    // May -> Not -> Have -> All -> Permissions! (the final existing folder is what matters)
                    //
                    if (Directory.Exists(parentDirectory))
                    {
                        parentDirectoryExists = true;
                        parentDirectoryPermissions = HasPermissions(parentDirectory, FileSystemRights.CreateDirectories | FileSystemRights.CreateFiles);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void MigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false)
        {
            try
            {
                // Procedure
                //
                // 1) Create Destination Directory
                // 2) Move File

                var directory = Path.GetDirectoryName(destinationPath);

                if (!DirectoryExists(directory))
                     CreateDirectory(directory);

                System.IO.File.Move(sourcePath, destinationPath, overwriteDestination);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error Migrating File:  {0}", LogMessageType.General, LogLevel.Error, ex, sourcePath);
                throw ex;
            }
        }

        public void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DirectoryExists(string path)
        {
            try
            {
                return Directory.Exists(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool FileExists(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool AreFriendlyPath(bool isFileName, params string[] filePath)
        {
            var friendlyPath = MakeFriendlyPath(isFileName, filePath);
            var friendlyPathParts = friendlyPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            var inputPath = filePath.Join("\\", x => x);
            var inputPathParts = inputPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            if (friendlyPathParts.Length != inputPathParts.Length)
                return false;

            for (int index = 0; index < friendlyPathParts.Length; index++)
            {
                if (inputPathParts[index] !=  friendlyPathParts[index]) 
                    return false;
            }

            return true;
        }

        public string MakeFriendlyPath(bool isFileName, params string[] filePath)
        {
            // MUST SPLIT THE FILE PIECES FOR SUB-DIRECTORIES
            var filePathParts = filePath.SelectMany(x => x.Split('\\', StringSplitOptions.RemoveEmptyEntries)).ToArray();

            var resultPath = new string[filePathParts.Length];

            for (int index = 0; index < filePathParts.Length; index++)
            {
                // File Name
                if (isFileName && index == filePathParts.Length - 1)
                    resultPath[index] = StripFileCharacters(filePathParts[index]);

                // Directory Part
                else
                {
                    // Check For Drive Label
                    if (Directory.GetLogicalDrives().Any(drive => drive == filePathParts[index] + "\\"))
                        resultPath[index] = filePathParts[index];

                    else
                        resultPath[index] = StripPathCharacters(filePathParts[index]);
                }

            }

            return System.IO.Path.Combine(resultPath);
        }

        private string StripFileCharacters(string fileName)
        {
            // Convert UTF-8 character sequences
            var result = Encoding.UTF8.GetString(Encoding.Default.GetBytes(fileName));

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();

            foreach (var invalidChar in invalidChars)
            {
                result = result.Replace(invalidChar.ToString(), string.Empty);
            }

            return result;
        }

        private string StripPathCharacters(string pathPart)
        {
            // Convert UTF-8 character sequences
            var result = Encoding.UTF8.GetString(Encoding.Default.GetBytes(pathPart));

            // NOTE*** This does not include all invalid path characters!!! 
            //
            //         Adding:  ['?', '!', '\'', '%', '$', '#', '@', '^', '&', '*', '(', ')', '+', '=']
            var invalidChars = System.IO.Path.GetInvalidPathChars()
                                             .Concat(new char[] { '?', '!', '\'', ':', ';', '\"', '%', '$', '#', '@', '^', '&', '*', '(', ')', '+', '=' });

            foreach (var invalidChar in invalidChars)
            {
                result = result.Replace(invalidChar.ToString(), string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Creates a path from the root -> leaf directory -> in order -> to make directory checking -> very simple.
        /// </summary>
        private string[] CreateDirectoryPath(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Trying to create directory path with an empty or invalid directory");

            var directoryParts = directory.Split("\\");
            var assembledDirectory = string.Empty;
            var result = new List<string>();

            // Verify Directory:  If Exists, Validate Permissions
            //
            foreach (var part in directoryParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                if (assembledDirectory == string.Empty)
                    assembledDirectory = part;
                else
                    assembledDirectory += "\\" + part;

                result.Add(assembledDirectory);
            }

            return result.ToArray();
        }

        private bool HasPermissions(string directory, FileSystemRights permissionsType)
        {
            try
            {
                var info = new DirectoryInfo(directory);

                foreach (AuthorizationRule rule in info.GetAccessControl()
                                                       .GetAccessRules(true, true, typeof(SecurityIdentifier)))
                {
                    if (rule is FileSystemAccessRule &&
                       (rule as FileSystemAccessRule).FileSystemRights == permissionsType)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
