using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Laminar.Domain;

namespace Laminar.Avalonia.Controls;

public partial class ItemPicker : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Spacing = 5
    });
    
    public static readonly StyledProperty<IReadOnlyItemCategory<object>> ItemsCategoryProperty 
        = AvaloniaProperty.Register<ItemPicker, IReadOnlyItemCategory<object>>(nameof(ItemsCategory));

    static ItemPicker()
    {
        ItemsCategoryProperty.Changed.AddClassHandler<ItemPicker>((p, args) => p.ItemsCategoryChanged(args));
        ItemsPanelProperty.OverrideDefaultValue<ItemPicker>(DefaultPanel);
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

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ItemPickerItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ItemPickerItem>(item, out recycleKey);
    }

    [RelayCommand]
    private void MenuItemSelected(IReadOnlyList<object> category)
    {
        ItemsSource = category;
    }
}

public interface IObjectItemCategory : IReadOnlyItemCategory<object>;