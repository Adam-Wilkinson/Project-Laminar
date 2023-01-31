using System;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.PluginFramework.NodeSystem;

public interface INodeRowClonerFactory
{
    public INodeRowCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : IConvertsToNodeRow;
}
