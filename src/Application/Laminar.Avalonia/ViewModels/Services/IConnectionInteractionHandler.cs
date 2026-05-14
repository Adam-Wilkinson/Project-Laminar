using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IConnectionInteractionHandler
{
    public bool HoverConnection(IIOConnector first, IIOConnector second);

    public void CancelConnection();
    
    public void ConfirmConnection();
}