using Avalonia;
using Laminar.Avalonia.NodeDisplaySystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using ReactiveUI;

namespace Laminar.Avalonia.Models;

public class NodeModel : AvaloniaObject
{
    public static readonly DirectProperty<NodeModel, IWrappedNode> CoreNodeProperty = AvaloniaProperty.RegisterDirect<NodeModel, IWrappedNode>(
        nameof(CoreNode), 
        x => x.CoreNode, 
        (o, e) => o.CoreNode = e,
        unsetValue: new TestNodeWrapper());

    private IWrappedNode _coreNode;

    public IWrappedNode CoreNode
    {
        get => _coreNode;
        set => SetAndRaise(CoreNodeProperty, ref _coreNode, value);
    }
}
