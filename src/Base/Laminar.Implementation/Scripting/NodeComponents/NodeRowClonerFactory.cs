using System;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeRowClonerFactory : INodeComponentClonerFactory
{
    public INodeComponentCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : INodeComponent => new NodeRowCloner<T>(cloner, startCount);
}
