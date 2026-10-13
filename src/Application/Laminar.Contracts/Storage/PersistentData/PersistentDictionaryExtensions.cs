namespace Laminar.Contracts.Storage.PersistentData;

public static class PersistentDictionaryExtensions
{
    extension(IPersistentDictionary persistentDictionary)
    {
        public T GetValueOrThrow<T>(string key) where T : notnull =>
            persistentDictionary[key].GetValue<T>().TryGetResult(out var result, out var error)
                ? result.Value
                : throw new Exception($"Unable to get value '{key}'", error);
    }
}