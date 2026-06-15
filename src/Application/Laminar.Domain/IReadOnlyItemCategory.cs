namespace Laminar.Domain;

public interface IReadOnlyItemCategory<out T>
{
    public IReadOnlyList<IReadOnlyItemCategory<T>> SubCategories { get; }

    public IReadOnlyList<T> Items { get; }

    public string CategoryName { get; }
}

public static class ReadOnlyItemCategoryExtensions
{
    extension<TSource>(IReadOnlyItemCategory<TSource> category)
    {
        public ItemCategory<TOutput> RecursiveMap<TOutput>(Func<TSource, TOutput> map)
        {
            return new ItemCategory<TOutput>(category.CategoryName,
                category.SubCategories.Select(x => x.RecursiveMap(map)).ToList(),
                category.Items.Select(map).ToList());
        }
    }
} 