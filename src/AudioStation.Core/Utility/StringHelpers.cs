using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioStation.Core.Utility
{
    public static class StringHelpers
    {
        /// <summary>
        /// Compares two strings ignoring case
        /// </summary>
        public static bool CompareIC(string? string1, string? string2)
        {
            return string.Compare(string1, string2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool ContainsIC(string? string1, string? string2)
        {
            if (string1 == null)
                return false;

            if (string2 == null)
                return false;

            return string1.Contains(string2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool RegexMatchIC(string? pattern, string? target)
        {
            if (pattern == null || target == null)
                return false;

            return Regex.Match(target, pattern, RegexOptions.IgnoreCase).Success;
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
    }
}
