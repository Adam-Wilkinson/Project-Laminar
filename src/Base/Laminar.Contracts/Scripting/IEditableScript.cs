using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Contracts.Scripting;

public interface IEditableScript : IScript
{
    public new IConnectionCollection Connections { get; }

    public new INodeCollection Nodes { get; }
}
