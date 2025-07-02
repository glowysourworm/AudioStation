using SimpleWpf.NativeIO;

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

        /// <summary>
        /// Removes un-friendly characters for the file path. (TODO: Conventions (enum) for how the user will like to use this)
        /// </summary>
        public static string MakeFriendlyPath(string filePath)
        {
            var result = filePath;
            var invalidChars = System.IO.Path.GetInvalidPathChars();

            foreach (var invalidChar in invalidChars)
            {
                result = result.Replace(invalidChar, '_');
            }

            return result;
        }

        public static string MakeFriendlyFileName(string fileName)
        {
            var result = fileName;
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();

            foreach (var invalidChar in invalidChars)
            {
                result = result.Replace(invalidChar, '_');
            }

            return result;
        }
    }
}
