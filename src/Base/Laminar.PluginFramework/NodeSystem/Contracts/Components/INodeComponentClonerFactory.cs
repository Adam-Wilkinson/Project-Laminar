using System;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Components;

public interface INodeComponentClonerFactory
{
    public INodeComponentCloner<T> CreateCloner<T>(Func<T> cloner, int startCount) where T : INodeComponent;
}
