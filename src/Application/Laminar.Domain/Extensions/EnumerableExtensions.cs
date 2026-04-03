using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;

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

    extension<T>(ICovariantObservableValue<IEnumerable<T>> observableEnumerable) where T : notnull
    {
        /// <summary>
        /// Converts an ObservableValue that contains a list to an INotifyCollectionChanged list,
        /// using a <see cref="SourcedObservableCollection{T}"/> to compute a minimal set of collection change actions
        /// </summary>
        /// <param name="comparer">The equality comparer used to compute collection changed actions</param>
        /// <returns></returns>
        public IReadOnlyObservableCollection<T> ToObservableCollection(IEqualityComparer<T>? comparer = null)
        {
            SourcedObservableCollection<T> output = new(observableEnumerable.Value, comparer);

            observableEnumerable.CovariantOnChanged += (_, _) =>
            {
                output.ChangeSourceTo(observableEnumerable.Value);
            };

            return output;
        }
    }
}
