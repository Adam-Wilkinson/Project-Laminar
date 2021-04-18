using System;
using System.Collections.Generic;
using System.Threading;
using Laminar_Core.NodeSystem;
using Laminar_Core.Primitives.UserInterface;
using Laminar_PluginFramework;
using Laminar_Core.PluginManagement;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core
{
    public class Instance
    {
        private readonly Dictionary<Type, TypeInfoRecord> TypeInfo = new();

        public Instance(SynchronizationContext uiContext)
        {
            Factory = new ObjectFactory(this);
            RegisteredEditors = Factory.GetImplementation<IUserInterfaceRegister>();
            RegisteredDisplays = Factory.GetImplementation<IUserInterfaceRegister>();
            LoadedNodeManager = Factory.CreateInstance<LoadedNodeManager>();
            Laminar.Init(Factory);
            UIContext = uiContext;
            _ = new PluginLoader(new PluginHost(this));
        }

        public IObjectFactory Factory { get; }

        public SynchronizationContext UIContext { get; }

        public LoadedNodeManager LoadedNodeManager { get; }

        public IUserInterfaceRegister RegisteredEditors { get; }

        public IUserInterfaceRegister RegisteredDisplays { get; }

        public bool RegisterTypeInfo(Type type, TypeInfoRecord record) => TypeInfo.TryAdd(type, record);

        public TypeInfoRecord GetTypeInfo(Type type)
        {
            if (TypeInfo.TryGetValue(type, out TypeInfoRecord info))
            {
                return info;
            }

            throw new NotSupportedException($"The type {type} is not registered");
        }

        public record TypeInfoRecord(object DefaultValue, string HexColour, string DefaultDisplay, string DefaultEditor, string UserFriendlyName);
    }
}
