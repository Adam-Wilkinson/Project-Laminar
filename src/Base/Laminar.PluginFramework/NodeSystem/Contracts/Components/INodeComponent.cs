using System;
using System.Collections.Generic;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Components;

public interface INodeComponent : IEnumerable<INodeComponent>
{
    public Opacity Opacity { get; }

    public event EventHandler<LaminarExecutionContext>? StartExecution;
}
