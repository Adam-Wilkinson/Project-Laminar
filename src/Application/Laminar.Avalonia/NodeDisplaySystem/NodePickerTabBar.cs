using System.Collections.Generic;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using ReactiveUI;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class NodePickerTabBar : TemplatedControl
{
    public static readonly StyledProperty<IReadOnlyList<IWrappedNode>> CurrentDisplayNodesProperty = AvaloniaProperty.Register<NodePickerTabBar, IReadOnlyList<IWrappedNode>>(nameof(CurrentDisplayNodes));
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NodePickerTabBar, Orientation>("Orientation", Orientation.Horizontal);
    public static readonly StyledProperty<IReadOnlyList<ItemCatagory<IWrappedNode>>> CatagoriesProperty = AvaloniaProperty.Register<NodePickerTabBar, IReadOnlyList<ItemCatagory<IWrappedNode>>>(nameof(Catagories));

    static NodePickerTabBar()
    {
        CatagoriesProperty.Changed.AddClassHandler<NodePickerTabBar>((tabBar, e) => { if (e.NewValue is IReadOnlyList<ItemCatagory<IWrappedNode>> newNodes) tabBar.CurrentDisplayNodes = newNodes[0].Items; });
    }

    public NodePickerTabBar()
    {
        SelectItem = ReactiveCommand.Create<List<IWrappedNode>>(nodes =>
        {
            CurrentDisplayNodes = nodes;
        });
    }

    public IReadOnlyList<ItemCatagory<IWrappedNode>> Catagories
    {
        get => GetValue(CatagoriesProperty);
        set => SetValue(CatagoriesProperty, value);
    }

    public IReadOnlyList<IWrappedNode> CurrentDisplayNodes
    {
        get => GetValue(CurrentDisplayNodesProperty);
        set => SetValue(CurrentDisplayNodesProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public ReactiveCommand<List<IWrappedNode>, Unit> SelectItem { get; }
}
