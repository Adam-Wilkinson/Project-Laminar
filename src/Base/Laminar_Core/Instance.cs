using System;
using System.Collections.Generic;
using System.Threading;
using Laminar_Core.NodeSystem;
using Laminar_Core.Primitives.UserInterface;
using Laminar_PluginFramework;
using Laminar_Core.PluginManagement;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using System.Linq;
using System.Collections.ObjectModel;

namespace Laminar_Core
{
    public class Instance
    {
        private readonly Dictionary<Type, TypeInfoRecord> _typeInfo = new();

        public Instance(SynchronizationContext uiContext)
        {
            Factory = new ObjectFactory(this);
            Laminar.Init(Factory);

            RegisteredEditors = Factory.GetImplementation<IUserInterfaceRegister>();
            RegisteredDisplays = Factory.GetImplementation<IUserInterfaceRegister>();
            LoadedNodeManager = Factory.CreateInstance<LoadedNodeManager>();

            _ = new PluginLoader(new PluginHost(this));

            AllScripts = new();

            ActiveNodeTree = Factory.GetImplementation<IObservableValue<INodeTree>>();
            ActiveNodeTree.Value = Factory.GetImplementation<INodeTree>();
            UIContext = uiContext;
            AllRegisteredTypes = _typeInfo.Values.Where(x => x.CanBeInput);
        }

        public ObservableCollection<INodeTree> AllScripts { get; }

        public IObservableValue<INodeTree> ActiveNodeTree { get; }

        public IObjectFactory Factory { get; }

        public SynchronizationContext UIContext { get; }

        public LoadedNodeManager LoadedNodeManager { get; }

        public IUserInterfaceRegister RegisteredEditors { get; }

        public IUserInterfaceRegister RegisteredDisplays { get; }

        public IEnumerable<TypeInfoRecord> AllRegisteredTypes { get; }

        public bool RegisterTypeInfo(Type type, TypeInfoRecord record) => _typeInfo.TryAdd(type, record);

        public TypeInfoRecord GetTypeInfo(Type type)
        {
            if (_typeInfo.TryGetValue(type, out TypeInfoRecord info))
            {
                return info;
            }

            throw new NotSupportedException($"The type {type} is not registered");
        }
    }

    public record TypeInfoRecord(Type Type, object DefaultValue, string HexColour, string DefaultDisplay, string DefaultEditor, string UserFriendlyName, bool CanBeInput);
}
