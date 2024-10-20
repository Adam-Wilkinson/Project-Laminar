using System;

namespace Laminar.PluginFramework.NodeSystem;

public interface ILaminarExecutionSource
{
    public event EventHandler<LaminarExecutionContext> ExecutionStarted;
}
