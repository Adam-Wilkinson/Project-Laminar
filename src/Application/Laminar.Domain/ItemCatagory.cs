namespace Laminar.Domain;

public class ItemCatagory<T> : IReadOnlyItemCatagory<T>
{
    public const char SeparationChar = '/';

    private readonly List<string> _subCatagoryNames = new();
    private readonly List<T> _items = new();
    private readonly List<ItemCatagory<T>> _subCatagories = new();

    public ItemCatagory(string catagoryName)
    {
        CatagoryName = catagoryName;
    }

    public IReadOnlyList<ItemCatagory<T>> SubCatagories => _subCatagories;

    public IReadOnlyList<T> Items => _items;

    public string CatagoryName { get; }

    public void AddItem(T item, string? subCatagoryPath)
    {
        if (subCatagoryPath is null or "")
        {
            _items.Add(item);
        }
        else
        {
            int splitIndex = subCatagoryPath.IndexOf(SeparationChar);
            if (splitIndex > -1)
            {
                AddToSubCatagory(subCatagoryPath[..splitIndex], subCatagoryPath[(splitIndex + 1)..], item);
            }
            else
            {
                AddToSubCatagory(subCatagoryPath, null, item);
            }
        }
    }

    private void AddToSubCatagory(string subCatagoryKey, string? pathInSubCatagory, T item)
    {
        int index = _subCatagoryNames.BinarySearch(subCatagoryKey);

        if (index < 0)
        {
            index = ~index;
            _subCatagoryNames.Insert(index, subCatagoryKey);
            _subCatagories.Insert(index, new ItemCatagory<T>(subCatagoryKey));
        }

        SubCatagories[index].AddItem(item, pathInSubCatagory);
    }
}
