using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Laminar.Domain;

namespace Laminar.Avalonia.Controls;

public partial class ItemPicker : ItemsControl
{
    public static readonly StyledProperty<IReadOnlyItemCategory<object>> ItemsCategoryProperty 
        = AvaloniaProperty.Register<ItemPicker, IReadOnlyItemCategory<object>>(nameof(ItemsCategory));

    static ItemPicker()
    {
        ItemsCategoryProperty.Changed.AddClassHandler<ItemPicker>((p, args) => p.ItemsCategoryChanged(args));
    }

    private void ItemsCategoryChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var currentCategory = args.GetNewValue<IReadOnlyItemCategory<object>>();
        IReadOnlyList<object>? initialItems = null;
        
        while (initialItems is null || initialItems.Count == 0)
        {
            initialItems = currentCategory.Items;
            if (currentCategory.SubCategories.Count == 0)
            {
                break;
            }
            currentCategory = currentCategory.SubCategories[0];
        }
        MenuItemSelected(initialItems);
        
    }

    public IReadOnlyItemCategory<object> ItemsCategory
    {
        get => GetValue(ItemsCategoryProperty);
        set => SetValue(ItemsCategoryProperty, value);
    }

    [RelayCommand]
    private void MenuItemSelected(IReadOnlyList<object> category)
    {
        ItemsSource = category;
    }
}

public interface IObjectItemCategory : IReadOnlyItemCategory<object>;