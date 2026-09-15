using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting;

public interface IWritableNodeTree : INodeTree, INodeCollection
{
    public bool TryConnect(IOutputConnector outputConnector, IInputConnector inputConnector, [NotNullWhen(true)] out IConnection? connection);
    
    public bool SeverConnection(IOutputConnector outputConnector, IInputConnector inputConnector);
}