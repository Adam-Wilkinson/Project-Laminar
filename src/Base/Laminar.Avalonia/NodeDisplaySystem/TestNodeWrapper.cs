using System.Collections.ObjectModel;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Attributes;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class TestNodeWrapper : IWrappedNode
{
    private readonly IWrappedNode _coreNode;

    public TestNodeWrapper()
    {
        _coreNode = App.LaminarInstance.ServiceProvider.GetService<INodeFactory>().WrapNode(new TestNode(), null);
    }

    public Identifier<IWrappedNode> Id => _coreNode.Id;

    public INodeRow NameRow => _coreNode.NameRow;

    public IReadOnlyObservableCollection<INodeRow> Rows => _coreNode.Rows;

    public ObservableValue<Point> Location => _coreNode.Location;

    public IWrappedNode Clone(INotificationClient<LaminarExecutionContext> notificationClient) => _coreNode.Clone(notificationClient);

    public void TriggerNotification(LaminarExecutionContext source)
    {
    }

    public void Update(LaminarExecutionContext context) => _coreNode.Update(context);

    private class TestNode : INode
    {
        [ShowInNode] readonly ValueInputRow<string> TestStringInput = new("Test Input", "Example");
        [ShowInNode] readonly ValueOutputRow<double> TestNumberOutput = new("Test Output", 5);

        public string NodeName { get; } = "Test";

        public void Evaluate()
        {
        }
    }
}