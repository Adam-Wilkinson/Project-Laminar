namespace Laminar.Domain.Extensions;

public static class DictionaryExtensions
{
    extension<TKey, TVal>(IDictionary<TKey, TVal> dictionary)
    {
        public TVal GetOrCreate(TKey key, Func<TKey, TVal> factory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            
            var newVal = factory(key);
            dictionary.Add(key, newVal);
            return newVal;
        }
    }
}