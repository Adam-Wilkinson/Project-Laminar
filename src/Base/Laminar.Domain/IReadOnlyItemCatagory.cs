namespace Laminar.Domain;

public interface IReadOnlyItemCatagory<T>
{
    public IReadOnlyList<ItemCatagory<T>> SubCatagories { get; }

    public IReadOnlyList<T> Items { get; }

    public string CatagoryName { get; }
}
