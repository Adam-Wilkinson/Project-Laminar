using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;

namespace Laminar.Contracts.Scripting;

public interface IEditableScript : IScript
{
    public new IConnectionCollection Connections { get; }

    public new IObservableCollection<IWrappedNode> Nodes { get; }
}
