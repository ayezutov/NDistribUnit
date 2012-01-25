using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NDistribUnit.Common.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class PathUtilities
    {
        private static Regex escapeRegex;
        /// <summary>
        /// Escapes the name of the file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string EscapeFileName(string name)
        {
            if (escapeRegex == null)
            {
                var invalidChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());
                escapeRegex = new Regex("[" + string.Join("|", invalidChars.Select(c => Regex.Escape(c.ToString(CultureInfo.InvariantCulture)))) + "]");
            }

            return escapeRegex.Replace(name, "_");
        }
    }
}