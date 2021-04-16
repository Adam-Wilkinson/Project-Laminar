using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public static class ComponentCollectionExtensions
    {
        public static TComponent WithElement<TComponent>(this TComponent builder, object key, INodeComponent value) where TComponent : INodeComponentDictionary
        {
            builder.Add(key, value);
            return builder;
        }
    }
}
