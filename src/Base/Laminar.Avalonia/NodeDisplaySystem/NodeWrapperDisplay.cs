using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class NodeWrapperDisplay : TemplatedControl
{
    public static readonly StyledProperty<IWrappedNode> CoreNodeProperty = AvaloniaProperty.Register<NodeWrapperDisplay, IWrappedNode>(nameof(CoreNode));

    public IWrappedNode CoreNode
    {
        get => GetValue(CoreNodeProperty);
        set => SetValue(CoreNodeProperty, value);
    }
}