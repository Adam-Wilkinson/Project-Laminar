using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Primitives;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Laminar_Core.NodeSystem.Nodes
{
    public interface INodeContainer
    {
        public static Dictionary<INode, INodeContainer> NodeBases { get; } = new();

        public static IAdvancedScriptInstance GetNodeInstance(INode node) => null;

        IObservableValue<bool> ErrorState { get; }

        IVisualNodeComponentContainer Name { get; }

        IEditableNodeLabel NameLabel { get; }

        bool HasFields { get; }

        INotifyCollectionChanged Fields { get; }

        IPoint Location { get; }

        bool IsLive { get; set; }

        void Update(IAdvancedScriptInstance instance);

        void SetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue, object value);

        object GetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue);

        INodeContainer DuplicateNode();
    }
}