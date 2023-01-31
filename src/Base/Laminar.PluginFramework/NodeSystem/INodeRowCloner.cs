using System.Collections.Generic;
using System.Collections.Specialized;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.PluginFramework.NodeSystem;

public interface INodeRowCloner<T> : IReadOnlyList<T>, IConvertsToNodeComponent
    where T : IConvertsToNodeRow
{
    public void RemoveRowAt(int index);
}
