using Avalonia;
using Avalonia.Controls.Primitives;

namespace Laminar.Avalonia.Controls;

public class ExpandCollapseChevron : TemplatedControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<ExpandCollapseChevron, bool>(nameof(IsExpanded));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}