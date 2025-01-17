namespace Laminar.Domain;

public class ItemCategory<T>(string categoryName) : IReadOnlyItemCategory<T>
{
    public const char SeparationChar = '/';

    private readonly List<string> _subCategoryNames = [];
    private readonly List<T> _items = [];
    private readonly List<ItemCategory<T>> _subCategories = [];

    public IReadOnlyList<ItemCategory<T>> SubCategories => _subCategories;

    public IReadOnlyList<T> Items => _items;

    public string CategoryName { get; } = categoryName;

    public void AddItem(T item, string? subCategoryPath)
    {
        if (subCategoryPath is null or "")
        {
            _items.Add(item);
        }
        else
        {
            var splitIndex = subCategoryPath.IndexOf(SeparationChar);
            if (splitIndex > -1)
            {
                AddToSubCategory(subCategoryPath[..splitIndex], subCategoryPath[(splitIndex + 1)..], item);
            }
            else
            {
                AddToSubCategory(subCategoryPath, null, item);
            }
        }
    }

    private void AddToSubCategory(string subCategoryKey, string? pathInSubCategory, T item)
    {
        var index = _subCategoryNames.BinarySearch(subCategoryKey);

        if (index < 0)
        {
            index = ~index;
            _subCategoryNames.Insert(index, subCategoryKey);
            _subCategories.Insert(index, new ItemCategory<T>(subCategoryKey));
        }

        SubCategories[index].AddItem(item, pathInSubCategory);
    }
}
