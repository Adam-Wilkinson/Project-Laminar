using System;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.PluginFramework.NodeSystem.Contracts;

public interface INodeRow
{
    public IInputConnector? InputConnector { get; }

    public IOutputConnector? OutputConnector { get; }

    public object CentralDisplay { get; }

    public void CopyValueTo(INodeRow nodeRow);

    public event EventHandler<LaminarExecutionContext> StartExecution;
}