using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.Exceptions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Implementation.Scripting.Connections;

public class ConnectorViewFactory : IConnectorViewFactory
{
    readonly IClassInstancer _classInstancer;

    readonly List<TypeConnectorInfo> _connectorFactories = new();

    public ConnectorViewFactory(IClassInstancer classInstancer)
    {
        _classInstancer = classInstancer;
    }

    public IConnectorView CreateConnector(INodeIO nodeIO)
    {
        foreach (var factory in _connectorFactories)
        {
            if (factory.IOType.IsAssignableFrom(nodeIO.GetType()))
            {
                IIOConnector connector = factory.CreateAction(nodeIO);
                return new ConnectorView(connector);
            }
        }

        throw new ConnectorNotFoundException(nodeIO.GetType());
    }

    public IConnectorView CreateConnector(IInput input) => CreateConnector((INodeIO)input);

    public IConnectorView CreateConnector(IOutput output) => CreateConnector((INodeIO)output);

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