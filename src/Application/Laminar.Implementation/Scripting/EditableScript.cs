using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class EditableScript : IEditableScript
{
    public EditableScript(IScriptExecutionManager executionManager, IConnectionCollection connectionCollection, INodeCollection nodeCollection)
    {
        ExecutionInstance = executionManager.CreateExecutionInstance(this);
        Connections = connectionCollection;
        Nodes = nodeCollection;
        NodeTree = new NodeTree(this);
    }

    public string Name { get; set; } = "Unnamed Script";

    public INodeTree NodeTree { get; }

    public IConnectionCollection Connections { get; }

    public INodeCollection Nodes { get; }

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }

    IReadOnlyObservableCollection<IWrappedNode> IScript.Nodes => Nodes;

    IReadOnlyObservableCollection<IConnection> IScript.Connections => Connections;
}
