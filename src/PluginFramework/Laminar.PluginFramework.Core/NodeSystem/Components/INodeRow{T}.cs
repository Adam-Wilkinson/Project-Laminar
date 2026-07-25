using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeRow<out T> : INodeRow where T : IInterfaceData
{
    public new T CentralDisplay { get; }
    
    IInterfaceData INodeRow.CentralDisplay => CentralDisplay;
}