﻿using System;
using System.Collections.Generic;
using System.Threading;
using Laminar_Core.NodeSystem;
using Laminar_Core.Primitives.UserInterface;
using Laminar_PluginFramework;
using Laminar_Core.PluginManagement;
using Laminar_PluginFramework.Primitives;
using System.Linq;
using Laminar_Core.Scripting;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced;
using System.Diagnostics;
using Laminar_Core.Serialization;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Serialization;
using System.IO;

namespace Laminar_Core
{
    public class Instance
    {
        public static HashSet<Type> InternalNodes { get; } = new();

        private readonly Dictionary<Type, TypeInfoRecord> _typeInfo = new();
        private readonly PluginLoader _pluginLoader;
        private readonly bool _isLoading;

        public Instance(SynchronizationContext uiContext, [CallerFilePath] string path = "")
        {
            UIContext = uiContext;
            Factory = new ObjectFactory(this);
            Laminar.Init(Factory);

            Serializer = Factory.GetImplementation<ISerializer>();
            UserData = Factory.GetImplementation<IUserDataStore>();
            RegisteredEditors = Factory.GetImplementation<IUserInterfaceRegister>();
            RegisteredDisplays = Factory.GetImplementation<IUserInterfaceRegister>();
            LoadedNodeManager = Factory.CreateInstance<LoadedNodeManager>();
            AllScripts = Factory.GetImplementation<IScriptCollection>();

            _pluginLoader = new PluginLoader(this, path);
            LoadedNodeManager.LoadedNodes.Sort();
            AllRegisteredTypes = _typeInfo.Values.Where(x => x.CanBeInput);

            _isLoading = true;
            foreach (var serializedScript in UserData.LoadAllFromFolder<ISerializedObject<IAdvancedScript>>("Scripts", "las"))
            {
                AllAdvancedScripts.Add(Serializer.Deserialize(serializedScript, null));
            }
            _isLoading = false;
        }

        public Type GetNodeType(string nodeName, string pluginName)
        {
            foreach (IRegisteredPlugin plugin in _pluginLoader.RegisteredPlugins)
            {
                if (plugin.PluginName == pluginName && plugin.RegisteredNodes.TryGetValue(nodeName, out Type type))
                {
                    return type;
                }
            }

            throw new Exception($"The node {nodeName} is not loaded");
        }

        public IRegisteredPlugin GetNodePlugin(INode node)
        {
            foreach (IRegisteredPlugin plugin in _pluginLoader.RegisteredPlugins)
            {
                if (plugin.ContainsNode(node))
                {
                    return plugin;
                }
            }

            throw new ArgumentException($"Couldn't find a plugin for node of type {node.GetType()}");
        }

        public ObservableCollection<IAdvancedScript> AllAdvancedScripts { get; } = new();

        public ISerializer Serializer { get; }

        public IUserDataStore UserData { get; }

        public IScriptCollection AllScripts { get; }

        public IObjectFactory Factory { get; }

        public SynchronizationContext UIContext { get; }

        public LoadedNodeManager LoadedNodeManager { get; }

        public IUserInterfaceRegister RegisteredEditors { get; }

        public IUserInterfaceRegister RegisteredDisplays { get; }

        public IEnumerable<TypeInfoRecord> AllRegisteredTypes { get; }

        public bool RegisterTypeInfo(Type type, TypeInfoRecord record) => _typeInfo.TryAdd(type, record);

        public void SaveScript(IAdvancedScript script)
        {
            if (_isLoading)
            {
                return;
            }

            UserData.Save($"Scripts/{script.Name.Value}.las", Serializer.Serialize(script));
        }

        public TypeInfoRecord GetTypeInfo(Type type)
        {
            if (_typeInfo.TryGetValue(type, out TypeInfoRecord info))
            {
                return info;
            }

            if (type.IsEnum)
            {
                return new TypeInfoRecord(type, default, "#000000", "StringDisplay", "EnumEditor", null, false);
            }

            throw new NotSupportedException($"The type {type} is not registered");
        }
    }

    public record TypeInfoRecord(Type Type, object DefaultValue, string HexColour, string DefaultDisplay, string DefaultEditor, string UserFriendlyName, bool CanBeInput);
}
