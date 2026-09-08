using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace AudioStation.Core.Utility.FileUtility
{
    public class FileHelpers
    {
        /// <summary>
        /// This regex will look for strings like:  "{({digit(s)})}" (this is MSFT's standard renumbering scheme)
        /// </summary>
        public static string UNIQUE_LABEL_NUMBERING_REGEX = @"[(]\d[)]";

        public static int CalculateCRC32(string filePath)
        {
            var crcHash = new System.IO.Hashing.Crc32();

            // CRC32 CALCULATION:  THIS IS NOT KEPT ON THE FILE SYSTEM
            var fileBytes = File.ReadAllBytes(filePath);

            crcHash.Append(fileBytes);

            return crcHash.GetCurrentHash(fileBytes);
        }

        public static bool HasWritePermissions(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);

                var accessRules = directoryInfo.GetAccessControl()
                                               .GetAccessRules(true, true, typeof(SecurityIdentifier));

                if (accessRules == null)
                    return false;

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                        continue;

                    if (rule.AccessControlType == AccessControlType.Allow)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking write permissions:  " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Checks to see if file has directory as an ancestor
        /// </summary>
        public static bool IsAncestor(string path, string directory)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Invalid file path:  FileHelpers.IsAncestor");

            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Invalid file path:  FileHelpers.IsAncestor");

            try
            {
                DirectoryInfo? parent = null;

                do
                {
                    parent = Directory.GetParent(path);

                    if (parent?.FullName == directory)
                        return true;

                } while (parent != null);

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces the current extension with the provided extension
        /// </summary>
        public static string ReplaceExtension(string fileName, string extension)
        {
            return Path.GetFileNameWithoutExtension(fileName) + extension;
        }

        /// <summary>
        /// Creates a unique label using the convention:  "Label" -> "Label (x)" where x is the next integer to create
        /// a unique string.
        /// </summary>
        public static string CreateUniqueLabel(string input, IEnumerable<string> usedLabels)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Invalid string input to CreateUniqueLabel");

            var output = input;
            var completed = false;

            var counter = 0;
            var renameFormat = "{0} ({1})";

            // Run until output has no conflicts
            //
            while (!completed)
            {
                var conflict = false;

                // Check next output for uniqueness
                foreach (var label in usedLabels)
                {
                    if (output == label)
                    {
                        var regexResult = Regex.Match(output, UNIQUE_LABEL_NUMBERING_REGEX);
                        var numberString = regexResult.Value?.Replace("(", "")?.Replace(")", "")?.Trim();

                        if (!string.IsNullOrWhiteSpace(numberString))
                            int.TryParse(numberString, out counter);

                        // Increment counter regardless (it should have previous attempt)
                        counter++;

                        // The format was found, so replace the number in parens
                        if (regexResult.Success)
                            output = label.Replace(regexResult.Value, "(" + counter + ")");

                        // The format was not found, so start numbering the conflict
                        else
                            output = string.Format(renameFormat, output, counter);

                        conflict = true;
                        break;
                    }
                }

                completed = !conflict;
            }

            return output;
        }
    }
}
