using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static string Join(this IEnumerable<string> strings, string separator = "")
    {
        return strings.Aggregate((a, b) => a + separator + b);
    }
}
