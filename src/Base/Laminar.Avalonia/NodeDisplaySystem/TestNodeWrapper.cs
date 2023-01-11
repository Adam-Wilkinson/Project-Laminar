using System.Collections.Generic;
using System.Collections.ObjectModel;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Primitives;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class TestNodeWrapper : IWrappedNode
{
    private IWrappedNode _coreNode;

    public TestNodeWrapper()
    {
        _coreNode = App.LaminarInstance.ServiceProvider.GetService<INodeFactory>().WrapNode(new TestNode(), null);
    }

    public Identifier<IWrappedNode> Id => _coreNode.Id;

    public IWrappedNodeRow NameRow => _coreNode.NameRow;

    public ObservableCollection<IWrappedNodeRow> Fields => _coreNode.Fields;

    public ObservableValue<Point> Location => _coreNode.Location;

    public IWrappedNode Clone(INotificationClient<LaminarExecutionContext> notificationClient) => _coreNode.Clone(notificationClient);

    public void TriggerNotification(LaminarExecutionContext source)
    {
    }

    public void Update(LaminarExecutionContext context) => _coreNode.Update(context);

    private class TestNode : INode
    {
        public readonly ValueInput<string> TestStringInput = new("Test Input", "Example");
        public readonly ValueOutput<double> TestNumberOutput = new("Test Output", 5);

        public string NodeName { get; } = "Test";

        public void Evaluate()
        {
        }
    }
}