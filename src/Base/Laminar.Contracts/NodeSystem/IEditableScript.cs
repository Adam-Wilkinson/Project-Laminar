using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Domain;

namespace Laminar.Contracts.NodeSystem;

public interface IEditableScript : IScript
{
    public new IConnectionCollection Connections { get; }

    public new IObservableCollection<INodeWrapper> Nodes { get; }
}
