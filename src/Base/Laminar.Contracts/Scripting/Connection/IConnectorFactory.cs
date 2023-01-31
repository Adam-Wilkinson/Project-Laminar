﻿using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectorFactory
{
    IInputConnector CreateConnector(IInput input);

    IOutputConnector CreateConnector(IOutput output);

    void RegisterInputConnector<TNodeInput, TConnectorInput>()
        where TNodeInput : IInput
        where TConnectorInput : IInputConnector<TNodeInput>;

    void RegisterOutputConnector<TNodeOutput, TConnectorOutput>()
        where TNodeOutput : IOutput
        where TConnectorOutput : IOutputConnector<TNodeOutput>;
}