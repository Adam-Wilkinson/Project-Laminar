using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Transformation;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification.Value;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.Markup;

public class ConnectorRenderTransformCalculator(object headerHeight) : MarkupExtension
{
    private static readonly ConditionalWeakTable<IWrappedNode, NodeModel> NodeModels = [];
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var provideValueTarget = serviceProvider.GetRequiredService<IProvideValueTarget>();
        
        if (provideValueTarget.TargetObject is not StyledElement targetElement)
        {
            throw new InvalidOperationException("This extension is only valid on styled elements");
        }

        if (targetElement.IsInitialized)
        {
            return GetBindingFromInitialized(targetElement);
        }
        
        var targetProperty = (AvaloniaProperty)provideValueTarget.TargetProperty;
        targetElement.Initialized += TargetVisualOnInitialized;
        return AvaloniaProperty.UnsetValue;

        void TargetVisualOnInitialized(object? sender, EventArgs e)
        {
            targetElement.Bind(targetProperty, GetBindingFromInitialized(targetElement));
        }
    }

    private BindingBase GetBindingFromInitialized(StyledElement targetElement)
    {
        if (targetElement.DataContext is null) return AvaloniaProperty.UnsetValue.AsStaticBinding();
        
        var targetConnector = FindConnectorFromTarget(targetElement);
        double xOffset = targetConnector is IInputConnector ? -16 : -1;
        var targetNode = FindNodeFrom(targetElement);
        NodeModel model = NodeModels.GetValue(targetNode, node => new NodeModel(node));

        double typedHeaderHeight = headerHeight switch
        {
            double doubleHeaderHeight => doubleHeaderHeight,
            StaticResourceExtension { ResourceKey: { } staticResourceKey } when targetElement.FindResource(staticResourceKey) is double heightResource => heightResource,
            _ => throw new InvalidCastException()
        };
        
        return model
            .GetFractionalOffsetObservable(targetConnector)
            .Map(d =>
            {
                var yPosition = typedHeaderHeight * (d - 1);
                var operations = TransformOperations.CreateBuilder(1);
                operations.AppendTranslate(xOffset, yPosition);
                return operations.Build();
            })
            .ToBinding();
    }
    
    private static IConnector FindConnectorFromTarget(object target)
    {
        if (target is Visual targetVisual && ConnectorRegistry.GetRegisteredConnector(targetVisual) is { } connector)
        {
            return connector;
        }

        if (target is StyledElement { DataContext: IConnector dataContextConnector })
        {
            return dataContextConnector;
        }

        throw new InvalidOperationException($"Unable to find connector for object {target}");
    }

    private static IWrappedNode FindNodeFrom(StyledElement target)
    {
        while (target.DataContext is not IWrappedNode)
        {
            target = target.Parent ?? throw new InvalidCastException("Unable to find target node");
        }

        return (IWrappedNode)target.DataContext!;
    }

    private class NodeModel
    {
        private readonly Dictionary<IConnector, ObservableValue<double>> _connectorOffsetObservables = [];
        private readonly IWrappedNode _node;

        public NodeModel(IWrappedNode node)
        {
            _node = node;
            _node.IsCollapsed.CovariantOnChanged += NodeIsCollapsedChanged;
        }
        
        public IObservableValue<double> GetFractionalOffsetObservable(IConnector connector)
        {
            if (_connectorOffsetObservables.TryGetValue(connector, out var offsetObservable)) return offsetObservable;
            
            offsetObservable = new ObservableValue<double>(0);
            _connectorOffsetObservables.Add(connector, offsetObservable);
            UpdateConnectorOffsets();
            connector.PropertyChanged += ConnectorPropertyChanged;

            return offsetObservable;
        }
        
        private void NodeIsCollapsedChanged(object? sender, EventArgs e)
        {
            UpdateConnectorOffsets();
        }

        private void ConnectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IConnector.Flags) || !_node.IsCollapsed.Value) return;
            UpdateConnectorOffsets();
        }

        private void UpdateConnectorOffsets()
        {
            if (!_node.IsCollapsed.Value)
            {
                foreach (var observable in _connectorOffsetObservables.Values)
                {
                    observable.Value = 1;
                }

                return;
            }
            
            
            int activeInputConnectors = 0;
            int activeOutputConnectors = 0;
            foreach (var row in _node.Rows)
            {
                if (ActiveConnector(row.InputConnector) is not null)
                {
                    activeInputConnectors++;
                }

                if (ActiveConnector(row.OutputConnector) is not null)
                {
                    activeOutputConnectors++;
                }
            }

            if (activeInputConnectors == 0 && activeOutputConnectors == 0) return;

            int currentActiveInputConnector = 0;
            int currentActiveOutputConnector = 0;
            foreach (var row in _node.Rows)
            {
                if (ActiveConnector(row.InputConnector) is { } inputConnector &&
                    _connectorOffsetObservables.TryGetValue(inputConnector, out var inputOffset))
                {
                    inputOffset.Value = (currentActiveInputConnector + 1.0) / (activeInputConnectors + 1.0); 
                    currentActiveInputConnector++;
                }

                if (ActiveConnector(row.OutputConnector) is { } outputConnector &&
                    _connectorOffsetObservables.TryGetValue(outputConnector, out var outputOffset))
                {
                    outputOffset.Value = (currentActiveOutputConnector + 1.0) / (activeOutputConnectors + 1.0);
                    currentActiveOutputConnector++;
                }
            }
        }
        
        private static IConnector? ActiveConnector(IConnector? connector)
            => connector is not null && connector.Flags.HasFlag(ConnectorFlags.HasConnections) ? connector : null;
    }
}