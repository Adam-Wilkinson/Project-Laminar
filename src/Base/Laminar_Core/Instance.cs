using System;
using System.Collections.Generic;
using System.Threading;
using Laminar_Core.NodeSystem;
using Laminar_Core.Primitives.UserInterface;
using Laminar_PluginFramework;
using Laminar_Core.PluginManagement;
using Laminar_PluginFramework.Primitives;
using System.Linq;
using Laminar_Core.Scripts;

namespace Laminar_Core
{
    public class Instance
    {
        private readonly Dictionary<Type, TypeInfoRecord> _typeInfo = new();

        public Instance(SynchronizationContext uiContext)
        {
            UIContext = uiContext;
            Factory = new ObjectFactory(this);
            Laminar.Init(Factory);

            RegisteredEditors = Factory.GetImplementation<IUserInterfaceRegister>();
            RegisteredDisplays = Factory.GetImplementation<IUserInterfaceRegister>();
            LoadedNodeManager = Factory.CreateInstance<LoadedNodeManager>();
            AllScripts = Factory.GetImplementation<IScriptCollection>();

            _ = new PluginLoader(new PluginHost(this));
            AllRegisteredTypes = _typeInfo.Values.Where(x => x.CanBeInput);
        }

        public IScriptCollection AllScripts { get; }

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
