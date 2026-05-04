namespace Laminar.Domain;

public interface IReadOnlyItemCategory<out T>
{
    public IReadOnlyList<IReadOnlyItemCategory<T>> SubCategories { get; }

    public IReadOnlyList<T> Items { get; }

    public string CategoryName { get; }
}
