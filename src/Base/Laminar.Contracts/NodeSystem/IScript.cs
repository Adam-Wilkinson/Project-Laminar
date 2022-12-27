using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Domain;

namespace Laminar.Contracts.NodeSystem;

public interface IScript
{
    public string Name { get; set; }

    public IReadOnlyObservableCollection<INodeWrapper> Nodes { get; }

    public IReadOnlyObservableCollection<IConnection> Connections { get; }

    public INodeTree NodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }
}
