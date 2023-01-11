using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Primitives;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNodeRow
{
    public IConnectorView? InputConnector { get; }

    public IConnectorView? OutputConnector { get; }

    public IDisplay Display { get; }

    public void CloneTo(IWrappedNodeRow wrapper);

    public void RefreshDisplay();
}
