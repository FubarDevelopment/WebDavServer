using System;
using System.Text.RegularExpressions;

namespace DecaTec.WebDav
{
    /// <summary>
    /// Class containing helper methods for text.
    /// </summary>
    public static class TextHelper
    {
        private static Regex unicodeRegex = new Regex(@"[^\u0000-\u007F]");

        /// <summary>
        /// Determines if a string contains raw Unicode.
        /// </summary>
        /// <param name="str">The string to check for raw Unicode characters.</param>
        /// <returns>True if the string contains raw Unicode, otherwise false (also if the string specified is an empty string).</returns>
        /// <remarks>If a string contains raw Unicode, this often indicates that there is a problem displaying the string in a readable notation (e.g. for non western characters).</remarks>
        public static bool StringContainsRawUnicode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            var str1 = unicodeRegex.Replace(str, String.Empty);
            return !str.Equals(str1);
        }
    }
}
