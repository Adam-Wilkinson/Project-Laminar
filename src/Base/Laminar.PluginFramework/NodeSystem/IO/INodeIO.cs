using System;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface INodeIO
{
    public Action? PreEvaluateAction { get; }

    public event EventHandler<LaminarExecutionContext> StartExecution;
}