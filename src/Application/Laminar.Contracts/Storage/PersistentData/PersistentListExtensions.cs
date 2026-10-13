namespace Laminar.Contracts.Storage.PersistentData;

public static class PersistentListExtensions
{
    public static T GetValueOrThrow<T>(this IPersistentList list, int index) where T : notnull =>
        list[index].GetValue<T>().TryGetResult(out var result, out var error) ? result.Value : throw error;
}