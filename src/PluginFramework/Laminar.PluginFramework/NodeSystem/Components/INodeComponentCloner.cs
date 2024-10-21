using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeComponentCloner<T> : INodeComponent, INotifyCollectionChanged
    where T : INodeComponent
{
    public new IEnumerator<T> GetEnumerator();

    public int Count { get; }

    public void RemoveRowAt(int index);
}
