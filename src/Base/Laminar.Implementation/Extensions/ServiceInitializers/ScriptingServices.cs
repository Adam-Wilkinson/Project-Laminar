using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.Connections;
using Laminar.Implementation.Scripting.Execution;
using Laminar.Implementation.Scripting.NodeComponents;
using Laminar.Implementation.Scripting.Nodes;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class ScriptingServices
{
    public static IServiceCollection AddScriptingServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INodeFactory, NodeFactory>();
        serviceCollection.AddSingleton<INodeRowFactory, NodeRowFactory>();
        serviceCollection.AddSingleton<INodeRowCollectionFactory, NodeRowCollectionFactory>();
        serviceCollection.AddSingleton<INodeComponentClonerFactory, NodeRowClonerFactory>();

        serviceCollection.AddSingleton<IScriptEditor, ScriptEditor>();
        serviceCollection.AddSingleton<IScriptFactory, ScriptFactory>();
        serviceCollection.AddSingleton<IScriptExecutionManager, ScriptExecutionManager>();
        serviceCollection.AddSingleton<IExecutionOrderFinder, ExecutionOrderFinder>();

        serviceCollection.AddSingleton<IConnectionBridger, DefaultConnectionBridger>();
        serviceCollection.AddSingleton<IConnectorFactory, ConnectorFactory>();
        serviceCollection.AddSingleton<IConnectorFactory, ConnectorFactory>();

        serviceCollection.AddSingleton<ILoadedNodeManager, LoadedNodeManager>();

        return serviceCollection;
    }
}
