namespace OpenFlow_Core.Management
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Registration;

    public class PluginManager : IPluginHost
    {
        public void LoadFromFolders(IEnumerable<string> folders)
        {
            if (folders == null)
            {
                throw new ArgumentNullException(nameof(folders));
            }

            foreach (string pluginFolder in folders)
            {
                if (PluginLoader.TryLoadFromFolder(pluginFolder, out IPlugin plugin))
                {
                    plugin.Register(this);
                }
            }

            foreach (IPlugin plugin in InbuiltPluginsLoader.GetInbuiltPlugins())
            {
                plugin.Register(this);
            }
        }

        public void AddNodeToMenu<TNode>(string menuItemName, string subItemName = null) where TNode : INode, new()
        {
            Instance.Current.LoadedNodeManager.AddNodeToCatagory<TNode>(menuItemName, subItemName);
        }

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

        public bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue = default, string defaultEditorName = null, string defaultDisplayName = null) => Instance.Current.RegisterTypeInfo(typeof(T), new Instance.TypeInfoRecord(defaultValue, hexColour, defaultDisplayName, defaultEditorName, userFriendlyName));

        public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode
        {
            throw new NotImplementedException();
        }

        public bool RegisterEditor<T>(string name, T editor) => Instance.Current.RegisteredEditors.RegisterUserInterface(name, editor);

        public bool RegisterDisplay<T>(string name, T display) => Instance.Current.RegisteredDisplays.RegisterUserInterface(name, display);

        public bool RegisterEditor<T>(string name, Type editorType) => Instance.Current.RegisteredEditors.RegisterUserInterface<T>(name, editorType);

        public bool RegisterDisplay<T>(string name, Type displayType) => Instance.Current.RegisteredDisplays.RegisterUserInterface<T>(name, displayType);
    }
}
