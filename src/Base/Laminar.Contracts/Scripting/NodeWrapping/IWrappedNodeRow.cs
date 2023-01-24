using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.Connection;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IWrappedNodeRow
{
    public IConnectorView? InputConnector { get; }

    public IConnectorView? OutputConnector { get; }

    public IDisplay Display { get; }

    public void CloneTo(IWrappedNodeRow wrapper);

    public void RefreshDisplay();
}
