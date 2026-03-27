namespace Laminar.Domain.Extensions;

public static class EnumerableExtensions
{
    extension<T>(T single)
    {
        public IEnumerable<T> Yield()
        {
            yield return single;
        }
    }
    
    extension<T>(IEnumerable<T> enumerable)
    {
        public IEnumerable<T> Flatten(Func<T, IEnumerable<T>> selector)
        {
            return enumerable.SelectMany(c => selector(c).Flatten(selector)).Concat(enumerable);
        }
    }

    extension<T>(IEnumerable<T>? nullable) where T : IEnumerable<T>
    {
        public IEnumerable<T> EmptyIfNull() => nullable ?? [];
    }
}
