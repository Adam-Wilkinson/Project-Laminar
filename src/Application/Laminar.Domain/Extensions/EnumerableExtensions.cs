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

    extension<TList, TValue>(TList? nullable) where TList : IEnumerable<TValue>
    {
        public IEnumerable<TValue> EmptyIfNull() => nullable ?? Enumerable.Empty<TValue>();
    }
}
