using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepResolve
{
    internal static class StringExtensions
    {
        public static string[] RemoveEmptyLines(this IEnumerable<string> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        }
    }
}
