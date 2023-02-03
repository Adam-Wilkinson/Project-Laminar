using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO;

namespace Laminar.Implementation.Scripting.Connections;

public class ConnectorFactory : IConnectorFactory
{
    readonly IClassInstancer _classInstancer;

    readonly List<TypeConnectorInfo> _connectorFactories = new();

    public ConnectorFactory(IClassInstancer classInstancer)
    {
        _classInstancer = classInstancer;
    }

    public IIOConnector CreateConnector(INodeIO nodeIO)
    {
        foreach (var factory in _connectorFactories)
        {
            if (factory.IOType.IsAssignableFrom(nodeIO.GetType()))
            {
                return factory.CreateAction(nodeIO);
            }
        }

        throw new ConnectorNotFoundException(nodeIO.GetType());
    }

    public IInputConnector CreateConnector(IInput input) => (IInputConnector)CreateConnector((INodeIO)input);

    public IOutputConnector CreateConnector(IOutput output) => (IOutputConnector)CreateConnector((INodeIO)output);

    public void RegisterInputConnector<TNodeInput, TConnectorInput>()
        where TNodeInput : IInput
        where TConnectorInput : IInputConnector<TNodeInput>
    {
        _connectorFactories.Add(new TypeConnectorInfo(typeof(TNodeInput), typeof(TConnectorInput), (nodeIO) =>
        {
            TConnectorInput newInstance = _classInstancer.CreateInstance<TConnectorInput>();
            newInstance.Init((TNodeInput)nodeIO);
            return newInstance;
        }));
    }

    public void RegisterOutputConnector<TNodeOutput, TConnectorOutput>()
        where TNodeOutput : IOutput
        where TConnectorOutput : IOutputConnector<TNodeOutput>
    {
        _connectorFactories.Add(new TypeConnectorInfo(typeof(TNodeOutput), typeof(TConnectorOutput), (nodeIO) =>
        {
            TConnectorOutput newInstance = _classInstancer.CreateInstance<TConnectorOutput>();
            newInstance.Init((TNodeOutput)nodeIO);
            return newInstance;
        }));
    }

    private record class TypeConnectorInfo(Type IOType, Type ConnectorType, Func<INodeIO, IIOConnector> CreateAction);
}