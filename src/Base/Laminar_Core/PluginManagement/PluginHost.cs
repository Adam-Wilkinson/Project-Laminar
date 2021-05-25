using System;
using Laminar_Core.Serialization;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Registration;
using Laminar_PluginFramework.Serialization;

namespace Laminar_Core.PluginManagement
{
    public class PluginHost : IPluginHost
    {
        private readonly Instance _instance;
        private readonly PluginLoader.RegisteredPlugin _registeredPlugin;

        public PluginHost(Instance instance, PluginLoader.RegisteredPlugin registeredPlugin)
        {
            _instance = instance;
            _registeredPlugin = registeredPlugin;
        }

        public void AddNodeToMenu<TNode>(string menuItemName, string subItemName = null) where TNode : INode, new()
        {
            TNode node = new();
            _registeredPlugin.RegisterNode(node);
            _instance.LoadedNodeManager.AddNodeToCatagory(node, menuItemName, subItemName);
        }

        public bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue, string defaultEditorName, string defaultDisplayName, bool isTreeInput, IObjectSerializer<T> serializer)
        {
            if (serializer is not null)
            {
                _instance.Serializer.RegisterSerializer(serializer);
            }
            return _instance.RegisterTypeInfo(typeof(T), new TypeInfoRecord(typeof(T), defaultValue, hexColour, defaultDisplayName, defaultEditorName, userFriendlyName, isTreeInput));
        }

        public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode
            => throw new NotImplementedException();

        public bool RegisterEditor<T>(string name, T editor)
            => _instance.RegisteredEditors.RegisterUserInterface(name, editor);

        public bool RegisterDisplay<T>(string name, T display)
            => _instance.RegisteredDisplays.RegisterUserInterface(name, display);

        public bool RegisterEditor<TBase, TEditor>(string name) where TEditor : TBase
            => _instance.RegisteredEditors.RegisterUserInterface<TBase>(name, typeof(TEditor));

        public bool RegisterDisplay<TBase, TDisplay>(string name) where TDisplay : TBase
            => _instance.RegisteredDisplays.RegisterUserInterface<TBase>(name, typeof(TDisplay));

        public void AddNodeToMenu<TNode1, TNode2>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
            AddNodeToMenu<TNode4>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
            AddNodeToMenu<TNode4>(menuItemName, subItemName);
            AddNodeToMenu<TNode5>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
            AddNodeToMenu<TNode4>(menuItemName, subItemName);
            AddNodeToMenu<TNode5>(menuItemName, subItemName);
            AddNodeToMenu<TNode6>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
            where TNode7 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
            AddNodeToMenu<TNode4>(menuItemName, subItemName);
            AddNodeToMenu<TNode5>(menuItemName, subItemName);
            AddNodeToMenu<TNode6>(menuItemName, subItemName);
            AddNodeToMenu<TNode7>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7, TNode8>(string menuItemName, string subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
            where TNode7 : INode, new()
            where TNode8 : INode, new()
        {
            AddNodeToMenu<TNode1>(menuItemName, subItemName);
            AddNodeToMenu<TNode2>(menuItemName, subItemName);
            AddNodeToMenu<TNode3>(menuItemName, subItemName);
            AddNodeToMenu<TNode4>(menuItemName, subItemName);
            AddNodeToMenu<TNode5>(menuItemName, subItemName);
            AddNodeToMenu<TNode6>(menuItemName, subItemName);
            AddNodeToMenu<TNode7>(menuItemName, subItemName);
            AddNodeToMenu<TNode8>(menuItemName, subItemName);
        }
    }
}
