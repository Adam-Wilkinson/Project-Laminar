using System;

namespace Laminar.PluginFramework.NodeSystem.IO;

public interface INodeIO : ILaminarExecutionSource
{
    public Action? PreEvaluateAction { get; }
}