﻿using System;
using System.Collections.Generic;
using System.Threading;
using Laminar_Core.Primitives.UserInterface;
using Laminar.Core.PluginManagement;
using Laminar_PluginFramework.Primitives;
using System.Linq;
using Laminar_Core.Scripting;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Laminar_Core.Scripting.Advanced;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Serialization;
using Laminar.Core.ScriptEditor.Nodes;
using Laminar.Contracts.UserInterface;
using Laminar.Contracts.NodeSystem;
using Microsoft.Extensions.DependencyInjection;
using Laminar.Contracts.ActionSystem;
using Laminar.Core.ScriptEditor.Actions;
using Laminar.Core.ScriptEditor.Connections;
using Laminar.Core.UserInterface;
using Laminar_PluginFramework.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.Core.UserPreferences;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Core.ScriptEditor;
using Laminar.PluginFramework.Registration;

namespace Laminar.Core;

public class Instance
{
    public static HashSet<Type> InternalNodes { get; } = new();

    private readonly Dictionary<Type, TypeInfoRecord> _typeInfo = new();
    private readonly PluginLoader _pluginLoader;
    private readonly bool _isLoading;

    public Instance(SynchronizationContext uiContext, FrontendDependency frontend, [CallerFilePath] string path = "")
    {
        UIContext = uiContext;
        Factory = new Laminar_Core.ObjectFactory(this);
        Laminar_PluginFramework.Laminar.Init(new Laminar_Core.ObjectFactory(this));

        Serializer = Factory.GetImplementation<ISerializer>();
        UserData = Factory.GetImplementation<IUserDataStore>();
        RegisteredEditors = Factory.GetImplementation<IUserInterfaceRegister>();
        RegisteredDisplays = Factory.GetImplementation<IUserInterfaceRegister>();
        AllScripts = Factory.GetImplementation<IScriptCollection>();

        _pluginLoader = new PluginLoader(path, frontend, ServiceProvider.GetService<IPluginHostFactory>());
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

    public IServiceProvider ServiceProvider { get; } = InitializeServices(new ServiceCollection()).BuildServiceProvider();

    public ISerializer Serializer { get; }

    public IUserDataStore UserData { get; }

    public IScriptCollection AllScripts { get; }

    public IObjectFactory Factory { get; }

    public SynchronizationContext UIContext { get; }

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

        return new TypeInfoRecord(type, default, default, default, default, default, default);
        // throw new NotSupportedException($"The type {type} is not registered");
    }

    private static IServiceCollection InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserInterfaceStore, UserInterfaceStore>();
        serviceCollection.AddSingleton<IReadOnlyUserInterfaceStore>(x => x.GetRequiredService<IUserInterfaceStore>());
        serviceCollection.AddSingleton<IUserInterfaceProvider, UserInterfaceProvider>();
        serviceCollection.AddSingleton<INodeFactory, NodeFactory>();
        serviceCollection.AddSingleton<INodeRowWrapperFactory, NodeRowWrapperFactory>();
        serviceCollection.AddSingleton<IValueDisplayFactory, ValueDisplayFactory>();
        serviceCollection.AddSingleton<IUserInterfaceFactory, UserInterfaceFactory>();

        serviceCollection.AddSingleton<IScriptExecutionManager, ScriptExecutionManager>();
        serviceCollection.AddSingleton<IConnectorViewFactory, ConnectorViewFactory>();
        serviceCollection.AddSingleton<ITypeInfoStore, TypeInfoStore>();
        serviceCollection.AddSingleton<IReadonlyTypeInfoStore>(x => x.GetRequiredService<ITypeInfoStore>());
        serviceCollection.AddSingleton<ILoadedNodeManager, LoadedNodeManager>();
        serviceCollection.AddSingleton<IUserActionManager, UserActionManager>();
        serviceCollection.AddSingleton<IScriptEditor, ScriptEditor.ScriptEditor>();
        serviceCollection.AddSingleton<IScriptFactory, ScriptFactory>();
        serviceCollection.AddSingleton<IConnectionBridger, DefaultConnectionBridger>();
        serviceCollection.AddSingleton<INodeRowCollectionFactory, NodeRowCollectionFactory>();
        serviceCollection.AddSingleton<IConnectorViewFactory, ConnectorViewFactory>();

        serviceCollection.AddSingleton<IUserPreferenceManager, UserPreferenceManager>();
        serviceCollection.AddSingleton<IClassInstancer, ClassInstancer>();

        serviceCollection.AddSingleton<IPluginHostFactory, PluginHostFactory>();

        return serviceCollection;
    }
}

public record TypeInfoRecord(Type Type, object DefaultValue, string HexColour, string DefaultDisplay, string DefaultEditor, string UserFriendlyName, bool CanBeInput);
