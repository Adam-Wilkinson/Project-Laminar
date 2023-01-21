using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class EditableScript : IEditableScript
{
    public EditableScript(IScriptExecutionManager executionManager, IConnectionCollection connectionCollection)
    {
        ExecutionInstance = executionManager.CreateExecutionInstance(this);
        Connections = connectionCollection;
        NodeTree = new NodeTree(this);
    }

    public string Name { get; set; }

    public INodeTree NodeTree { get; }

    public IConnectionCollection Connections { get; }

    public IObservableCollection<IWrappedNode> Nodes { get; } = new LaminarObservableCollection<IWrappedNode>();

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }

    IReadOnlyObservableCollection<IWrappedNode> IScript.Nodes => Nodes;
    IReadOnlyObservableCollection<IConnection> IScript.Connections => Connections;
}
