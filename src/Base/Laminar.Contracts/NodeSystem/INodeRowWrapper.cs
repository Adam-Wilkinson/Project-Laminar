using System.ComponentModel;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;

namespace Laminar.Contracts.NodeSystem;

public interface INodeRowWrapper
{
    public IConnectorView? InputConnector { get; }

    public IConnectorView? OutputConnector { get; }

    public IValueDisplay Display { get; }

    public void CloneTo(INodeRowWrapper wrapper);

    public void RefreshDisplay();
}
