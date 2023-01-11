using System.Collections.Generic;

namespace Laminar.Implementation.Extensions;

public static class YieldItemAsEnumerable
{
    public static IEnumerable<T> Yield<T>(this T item)
    {
        if (item is null)
        {
            yield break;
        }

        yield return item;
    }
}
