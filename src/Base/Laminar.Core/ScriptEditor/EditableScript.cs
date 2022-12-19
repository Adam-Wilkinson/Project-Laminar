using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Domain;

namespace Laminar.Core.ScriptEditor;

internal class EditableScript : IEditableScript
{
    public EditableScript(
        IScriptExecutionManager executionManager,
        IConnectionCollection connectionCollection)
    {
        ExecutionInstance = executionManager.CreateExecutionInstance(this);
        Connections = connectionCollection;
        NodeTree = new NodeTree(this);
    }

    public string Name { get; set; }

    public INodeTree NodeTree { get; }

    public IConnectionCollection Connections { get; }

    public IObservableCollection<INodeWrapper> Nodes { get; } = new ObservableCollection<INodeWrapper>();

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }

    IReadOnlyObservableCollection<INodeWrapper> IScript.Nodes => Nodes;
    IReadOnlyObservableCollection<IConnection> IScript.Connections => Connections;
}
