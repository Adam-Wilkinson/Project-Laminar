using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Laminar.Domain;

namespace Laminar.Avalonia.Controls;

[TemplatePart("PART_ScrollViewer", typeof(IScrollable))]
public partial class ItemPicker : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new VirtualizingStackPanel
    {
        Orientation = Orientation.Horizontal,
        Height = 60,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        ClipToBounds = false,
    });
    
    public static readonly StyledProperty<IReadOnlyItemCategory<object>> ItemsCategoryProperty 
        = AvaloniaProperty.Register<ItemPicker, IReadOnlyItemCategory<object>>(nameof(ItemsCategory));

    /// <summary>
    /// Defines the <see cref="Scroll"/> property.
    /// </summary>
    public static readonly DirectProperty<ItemPicker, IScrollable?> ScrollProperty =
        AvaloniaProperty.RegisterDirect<ItemPicker, IScrollable?>(nameof(Scroll), o => o.Scroll);

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

    public IScrollable? Scroll
    {
        get;
        private set => SetAndRaise(ScrollProperty, ref field, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Scroll = e.NameScope.Find<IScrollable>("PART_ScrollViewer");
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