using System.Collections.Generic;
using System.Collections.ObjectModel;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar_Core;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class TestNodeWrapper : INodeWrapper
{
    private INodeWrapper _coreNode;

    public TestNodeWrapper()
    {
        _coreNode = new ObjectFactory(null).GetImplementation<INodeFactory>().WrapNode(new TestNode(), null);
    }

    public Identifier<INodeWrapper> Id => _coreNode.Id;

    public INodeRowWrapper NameRow => _coreNode.NameRow;

    public ObservableCollection<INodeRowWrapper> Fields => _coreNode.Fields;

    public ObservableValue<Point> Location => _coreNode.Location;

    public INodeWrapper Clone(INotificationClient<LaminarExecutionContext> notificationClient) => _coreNode.Clone(notificationClient);

    public void TriggerNotification(LaminarExecutionContext source)
    {
    }

    public void Update(LaminarExecutionContext context) => _coreNode.Update(context);

    private class TestNode : INode
    {
        public readonly ValueInput<string> TestStringInput = new("Test Input", "Example");
        public readonly ValueOutput<double> TestNumberOutput = new("Test Output", 5);

        public IEnumerable<INodeComponent> Fields => null;

        public string NodeName { get; } = "Test";

        public void Evaluate()
        {
        }
    }
}