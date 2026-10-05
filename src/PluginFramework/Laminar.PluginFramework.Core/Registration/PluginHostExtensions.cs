using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.Registration;

public static class PluginHostExtensions
{
    extension(IPluginHost pluginHost)
    {
        public bool RegisterDataInterface<TInterfaceDefinition, TData, TInterface>() 
            where TInterfaceDefinition : IUserInterfaceDefinition, new() 
            where TData : notnull
            where TInterface : class, new()
        {
            pluginHost.RegisterDataInterfaceFactory<TInterfaceDefinition, TData, TInterface>(() => new TInterface());
            return true;
        }
        
        public void AddNodeToMenu<TNode1, TNode2>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode4>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode4>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode5>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode4>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode5>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode6>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
            where TNode7 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode4>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode5>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode6>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode7>(menuItemName, subItemName);
        }

        public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7, TNode8>(string menuItemName, string? subItemName = null)
            where TNode1 : INode, new()
            where TNode2 : INode, new()
            where TNode3 : INode, new()
            where TNode4 : INode, new()
            where TNode5 : INode, new()
            where TNode6 : INode, new()
            where TNode7 : INode, new()
            where TNode8 : INode, new()
        {
            pluginHost.AddNodeToMenu<TNode1>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode2>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode3>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode4>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode5>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode6>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode7>(menuItemName, subItemName);
            pluginHost.AddNodeToMenu<TNode8>(menuItemName, subItemName);
        }
    }
}