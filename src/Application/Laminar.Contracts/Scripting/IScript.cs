using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;

namespace Laminar.Contracts.Scripting;

public interface IScript
{
    public string Name { get; set; }

    public IReadOnlyObservableCollection<IWrappedNode> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }

    public INodeTree NodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }
}
