using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class AdvancedScriptInputsDisplay : TemplatedControl
{
    public static readonly StyledProperty<ObservableCollection<IWrappedNode>> InputNodesProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, ObservableCollection<IWrappedNode>>(nameof(InputNodes));
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, Orientation>(nameof(Orientation), Orientation.Vertical);
    public static readonly StyledProperty<IEnumerable<TypeInfo>> AllTypeInfoProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, IEnumerable<TypeInfo>>(nameof(AllTypeInfo));

    private readonly INodeFactory _nodeFactory;

    private ToggleButton _toggleAddMenuButton;
    private Vector _dragOffset;

    public AdvancedScriptInputsDisplay()
    {
        _nodeFactory = App.LaminarInstance.ServiceProvider.GetService<INodeFactory>();
        InputNodes = new();
        DataContextChanged += NodeTreeInputDisplay_DataContextChanged;

        // AllTypeInfo = App.LaminarInstance.AllRegisteredTypes;
    }

    public IEnumerable<TypeInfo> AllTypeInfo
    {
        get => GetValue(AllTypeInfoProperty);
        set => SetValue(AllTypeInfoProperty, value);
    }

    public ObservableCollection<IWrappedNode> InputNodes
    {
        get => GetValue(InputNodesProperty);
        set => SetValue(InputNodesProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public void AddInputOfType(Type type)
    {
        // (DataContext as IAdvancedScriptInputs).Add(type);
        _toggleAddMenuButton.IsChecked = false;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _toggleAddMenuButton = e.NameScope.Find<ToggleButton>("PART_AddNodeButton");
    }

    private void RemoveNode(object sender, INode item)
    {
    }

    private void AddNode(object sender, INode inputNode)
    {
        //INodeWrapper nodeContainer = _nodeFactory.WrapNode(inputNode, null);
        //InputNodes.Add(nodeContainer);
    }

    private void NodeTreeInputDisplay_DataContextChanged(object sender, EventArgs e)
    {
        //if (DataContext is IAdvancedScriptInputs inputs)
        //{
        //    inputs.NodeAdded += AddNode;
        //    inputs.NodeRemoved += RemoveNode;
        //    foreach (var inputNode in inputs.InputNodes)
        //    {
        //        AddNode(null, inputNode);
        //    }
        //}
    }
}
