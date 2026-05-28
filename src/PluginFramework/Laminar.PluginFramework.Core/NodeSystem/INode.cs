using System.Collections;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem.Components;

namespace Laminar.PluginFramework.NodeSystem;

public interface INode
{
    IEnumerable<INodeComponent> Components { get; }

    string NodeName { get; }

    void Evaluate();
}