using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Execution;

public interface INodeTree : INodeTreeView
{
    public void AddNode(IWrappedNode node);

    public bool DeleteNode(IWrappedNode node);
    
    public bool TryConnect(IOutputConnector outputConnector, IInputConnector inputConnector, [NotNullWhen(true)] out IConnection? connection);

    public bool ConnectionExists(IOutputConnector outputConnector, IInputConnector inputConnector);
    
    public bool SeverConnection(IOutputConnector outputConnector, IInputConnector inputConnector);
}