using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Utility.FileUtility
{
    public static class MigrationHelpers
    {
        public static string FormatLeftToken = "{";
        public static string FormatRightToken = "}";

        /// <summary>
        /// Extra format token:  extension
        /// </summary>
        private static LibraryFormatAttribute FileExtension = new LibraryFormatAttribute()
        {
            Name = "File Extension",
            Description = "This may be used to represent the file extension for the file",
            TokenName = "extension",
            Use = LibraryFormatUse.File
        };

        public static char[] GetInvalidFileCharacters()
        {
            // NOTE*** This does not include all invalid path characters!!! 
            //
            //         Adding:  ['?', '\'', '%', '$', '#', '@', '^', '&', '*', '(', ')', '+', '=']
            //
            return System.IO.Path.GetInvalidPathChars()
                                 .Concat(new char[] { '?', '\'', ':', ';', '\"', '%', '$', '#', '@', '^', '&', '*', '(', ')', '+', '=' })
                                 .ToArray();
        }

        public static LibraryFormat GetFormat<T>(LibraryFormatUse use, params LibraryExtraneousFields[] extraFields)
        {
            var keywords = typeof(T).GetProperties()
                                    .Select(propertyInfo =>
                                    {
                                        var formatAttribute = propertyInfo.GetCustomAttribute<LibraryFormatAttribute>();

                                        if (formatAttribute == null)
                                            return null;

                                        return new LibraryFormatField()
                                        {
                                            IsExtraneous = false,
                                            Description = formatAttribute.Description,
                                            Name = formatAttribute.Name,
                                            PropertyName = propertyInfo.Name,
                                            TokenName = formatAttribute.TokenName,
                                            Use = formatAttribute.Use
                                        };

                                    }).Where(x => x != null && x.Use.Has(use))
                                      .ToList();


            foreach (var enumValue in extraFields)
            {
                switch (enumValue)
                {
                    case LibraryExtraneousFields.FileExtension:
                        keywords.Add(new LibraryFormatField()
                        {
                            IsExtraneous = true,
                            Name = FileExtension.Name,
                            Description = FileExtension.Description,
                            PropertyName = string.Empty,
                            TokenName = FileExtension.TokenName,
                            Use = LibraryFormatUse.File
                        });
                        break;
                    default:
                        throw new Exception("Unhandled extra field type");
                }
            }


            return new LibraryFormat()
            {
                Fields = keywords,
                Use = use
            };
        }

        /// <summary>
        /// Applies the ILibraryFormat to the source string using a source object to produce a result string
        /// </summary>
        /// <param name="format">Format information:  how to draw data from the source object</param>
        /// <param name="source">Source (object):  properties are used (from the ILibraryFormat) to provide value strings</param>
        /// <param name="sourceFormatString">Inpupt string to operate on</param>
        /// <param name="extraneousFieldGetter">Getter for extraneous fields</param>
        /// <returns>Formatted string with the result data</returns>
        public static string Format(ILibraryFormat format, string sourceFormatString, object source, Func<LibraryExtraneousFields, string> extraneousFieldGetter = null)
        {
            if (string.IsNullOrWhiteSpace(sourceFormatString))
                throw new ArgumentException("Invalid format string");

            if (format == null ||
                source == null)
                throw new ArgumentNullException("Invalid format input");

            var result = sourceFormatString;

            foreach (var field in format.Fields)
            {
                // Field Property (token)
                var fieldTokenized = format.LeftToken + field.TokenName + format.RightToken;

                // Check for token
                if (result.Contains(fieldTokenized))
                {
                    // Property
                    if (!field.IsExtraneous)
                    {
                        // Field Property (value)
                        var propertyValue = source.GetProperty(field.PropertyName)?.ToString() ?? string.Empty;

                        // Field (value) Replacement
                        result = result.Replace(fieldTokenized, propertyValue);
                    }

                    // Extraneous
                    else if (extraneousFieldGetter != null)
                    {
                        // Field (value)
                        var propertyValue = extraneousFieldGetter(field.ExtraneousType);

                        // Field (value) Replacement
                        result = result.Replace(fieldTokenized, propertyValue);
                    }

                    else
                        throw new Exception("Unable to resolve format field:  " + field.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks all possible file movement issues (that are allowed without yet moving the file); and returns the result. DOES NOT
        /// CREATE / DELETE ANY FILES OR FOLDERS.
        /// </summary>
        public static bool CanMigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false)
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

        /// <summary>
        /// Moves file, checking parts of directory paths, checking folder permissions, and logging details. Returns true if 
        /// the file is moved successfully.
        /// </summary>
        public static void MigrateFile(string sourcePath, string destinationPath, bool overwriteDestination = true, bool deleteSource = true, bool deleteSourceEmptyFolder = false)
        {
            try
            {
                // Procedure
                //
                // 1) Create Destination Directory
                // 2) Move File

                var directory = Path.GetDirectoryName(destinationPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                System.IO.File.Move(sourcePath, destinationPath, overwriteDestination);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error Migrating File:  {0}", LogLevel.Error, ex, sourcePath);
                throw ex;
            }
        }

        public static bool AreFriendlyPath(bool isFileName, params string[] filePath)
        {
            var friendlyPath = MakeFriendlyPath(isFileName, filePath);
            var friendlyPathParts = friendlyPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            var inputPath = filePath.Join("\\", x => x);
            var inputPathParts = inputPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            if (friendlyPathParts.Length != inputPathParts.Length)
                return false;

            for (int index = 0; index < friendlyPathParts.Length; index++)
            {
                if (inputPathParts[index] != friendlyPathParts[index])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes un-friendly characters for the file path. Use for 
        /// directory names / file paths. The file name has a different illegal character set - so set 
        /// isFileName = true (iff) the final string is a file name. Otherwise, set it to false, and the 
        /// path will be processed according to what is "legal" for a directory (and, in this case, "friendly"). 
        /// Also, decodes UTF-8 escape sequences.
        /// </summary>
        public static string MakeFriendlyPath(bool isFileName, params string[] filePath)
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

        private static string StripFileCharacters(string fileName)
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

        private static string StripPathCharacters(string pathPart)
        {
            // Convert UTF-8 character sequences
            var result = Encoding.UTF8.GetString(Encoding.Default.GetBytes(pathPart));

            foreach (var invalidChar in GetInvalidFileCharacters())
            {
                result = result.Replace(invalidChar.ToString(), string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Creates a path from the root -> leaf directory -> in order -> to make directory checking -> very simple.
        /// </summary>
        private static string[] CreateDirectoryPath(string directory)
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

        private static bool HasPermissions(string directory, FileSystemRights permissionsType)
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
