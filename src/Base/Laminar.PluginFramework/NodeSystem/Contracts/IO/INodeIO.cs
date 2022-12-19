using System;

namespace Laminar.PluginFramework.NodeSystem.Contracts.IO;

public interface INodeIO
{
    public Action? PreEvaluateAction { get; }

    public event EventHandler<LaminarExecutionContext> StartExecution;
}