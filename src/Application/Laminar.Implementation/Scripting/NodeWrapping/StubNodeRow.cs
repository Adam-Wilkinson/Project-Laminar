using System.Collections;
using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class StubNodeRow : INodeRow
{
    private readonly StubInputConnector _inputConnector = new();
    private readonly StubOutputConnector _outputConnector = new();
    
    private static readonly InterfaceData<StringViewer, string> BlankInterfaceData = new("")
    {
        Name = "",
    };
    
    public IEnumerator<INodeComponent> GetEnumerator()
    {
        yield return this;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Opacity Opacity { get; } = new();

    public IInputConnector? InputConnector => _inputConnector;

    public IOutputConnector? OutputConnector => _outputConnector;

    public IInterfaceData CentralDisplay => BlankInterfaceData;
    
    public event EventHandler<LaminarExecutionContext>? StartExecution { add { } remove { } }

    public void LockConnectors()
    {
        _inputConnector.Lock();
        _outputConnector.Lock();
    }
    
    private class StubInputConnector : StubConnector, IInputConnector
    {
        public void OnConnectedTo(IOutputConnector output)
        {
        }

        public void OnDisconnectedFrom(IOutputConnector output)
        {
        }

        public bool CanConnectTo(IOutputConnector output) => !IsLocked;
    }

    private class StubOutputConnector : StubConnector, IOutputConnector
    {
        public PassUpdateOption PassUpdate(ExecutionFlags executionFlags) => PassUpdateOption.NeverPasses;

        public void OnConnectedTo(IInputConnector input)
        {
        }

        public void OnDisconnectedFrom(IInputConnector input)
        {
        }

        public bool CanConnectTo(IInputConnector input) => !IsLocked;
    }

    private class StubConnector : IConnector
    {
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
        public ConnectorFlags Flags => IsLocked ? ConnectorFlags.None : ConnectorFlags.AcceptsConnections;
        public Action? PreEvaluateAction => null;
        public string ColorHex => "#000000";
        protected bool IsLocked { get; private set; }
        
        public void Lock()
        {
            IsLocked = true;
        }
    }
}