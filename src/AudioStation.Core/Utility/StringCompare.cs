namespace AudioStation.Core.Utility
{
    public static class StringCompare
    {
        /// <summary>
        /// Compares two strings ignoring case
        /// </summary>
        public static bool CompareIC(string? string1, string? string2)
        {
            return string.Compare(string1, string2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
