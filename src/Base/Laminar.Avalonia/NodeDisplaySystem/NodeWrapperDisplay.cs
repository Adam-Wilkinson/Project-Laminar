using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia;
using Laminar.Contracts.NodeSystem;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class NodeWrapperDisplay : TemplatedControl
{
    public static readonly StyledProperty<INodeWrapper> CoreNodeProperty = AvaloniaProperty.Register<NodeWrapperDisplay, INodeWrapper>(nameof(CoreNode));

    public INodeWrapper CoreNode
    {
        get => GetValue(CoreNodeProperty);
        set => SetValue(CoreNodeProperty, value);
    }
}