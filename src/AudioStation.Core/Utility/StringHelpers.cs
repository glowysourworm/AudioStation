﻿using System.IO;
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
    }
}
