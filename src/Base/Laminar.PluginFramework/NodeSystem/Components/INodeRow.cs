using System;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeRow : INodeComponent
{
    public IInputConnector? InputConnector { get; }

    public IOutputConnector? OutputConnector { get; }

    public object CentralDisplay { get; }

    public void CopyValueTo(INodeRow nodeRow);
}