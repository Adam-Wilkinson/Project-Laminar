using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Scripting.Connections;
using Laminar.Implementation.Scripting;
using Laminar.Implementation.Scripting.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Implementation.Scripting.Execution;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

internal static class ScriptingServices
{
    public static IServiceCollection AddScriptingServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<INodeFactory, NodeFactory>();
        serviceCollection.AddSingleton<INodeRowWrapperFactory, NodeRowWrapperFactory>();
        serviceCollection.AddSingleton<INodeRowCollectionFactory, NodeRowCollectionFactory>();

        serviceCollection.AddSingleton<IScriptEditor, ScriptEditor>();
        serviceCollection.AddSingleton<IScriptFactory, ScriptFactory>();
        serviceCollection.AddSingleton<IScriptExecutionManager, ScriptExecutionManager>();

        serviceCollection.AddSingleton<IConnectionBridger, DefaultConnectionBridger>();
        serviceCollection.AddSingleton<IConnectorViewFactory, ConnectorViewFactory>();
        serviceCollection.AddSingleton<IConnectorViewFactory, ConnectorViewFactory>();

        serviceCollection.AddSingleton<ILoadedNodeManager, LoadedNodeManager>();

        return serviceCollection;
    }
}
