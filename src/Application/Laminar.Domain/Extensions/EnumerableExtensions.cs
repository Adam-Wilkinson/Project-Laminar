using Laminar.Domain.Observables;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;

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

        public IEnumerable<T> InsertInBetween(T newElement)
        {
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            yield return enumerator.Current;

            while (enumerator.MoveNext())
            {
                yield return newElement;
                yield return enumerator.Current;
            }
            
            enumerator.Dispose();
        }

        public int FindIndex(Predicate<T> predicate)
        {
            if (enumerable is List<T> list) return list.FindIndex(predicate);

            var index = 0;
            foreach (var item in enumerable)
            {
                if (predicate(item)) return index;
                index++;
            }

            return -1;
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
        /// using a <see cref="SourcedObservableList{T}"/> to compute a minimal set of collection change actions
        /// </summary>
        /// <param name="comparer">The equality comparer used to compute collection changed actions</param>
        /// <returns></returns>
        public IReadOnlyObservableList<T> ToObservableCollection(IEqualityComparer<T>? comparer = null)
        {
            SourcedObservableList<T> output = new(observableEnumerable.Value, comparer);

            observableEnumerable.CovariantOnChanged += (_, _) =>
            {
                output.ChangeSourceTo(observableEnumerable.Value);
            };

            return output;
        }
    }
}
