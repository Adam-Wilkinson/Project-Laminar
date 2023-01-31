using System;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal class NodeRowClonerFactory : INodeRowClonerFactory
{
    private readonly INodeRowFactory _rowFactory;

    public NodeRowClonerFactory(INodeRowFactory rowFactory)
    {
        _rowFactory = rowFactory;
    }

    public INodeRowCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : IConvertsToNodeRow => new NodeRowCloner<T>(_rowFactory, cloner, startCount);
}
