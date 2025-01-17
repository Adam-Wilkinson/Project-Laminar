namespace Laminar.Domain;

public interface IReadOnlyItemCategory<T>
{
    public IReadOnlyList<ItemCategory<T>> SubCategories { get; }

    public IReadOnlyList<T> Items { get; }

    public string CategoryName { get; }
}
