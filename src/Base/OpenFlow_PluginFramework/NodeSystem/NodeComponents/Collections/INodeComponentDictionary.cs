using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public interface INodeComponentDictionary : INodeComponentCollection, IDictionary<object, INodeComponent>
    {
        void HideAllComponents();

        bool HideComponentByKey(object key);

        bool ShowSectionByKey(object key);
    }
}