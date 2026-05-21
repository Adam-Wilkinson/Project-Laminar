using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IConnectionInteractionHandler
{
    /// <summary>
    /// Resolves the connector that is the true target of an interaction with a given connector
    /// </summary>
    /// <param name="connector">The connector that was interacted with</param>
    /// <returns>The connector that is being targeted</returns>
    public IIOConnector? GetTargetConnector(IIOConnector connector);
    
    public bool HoverConnection(IIOConnector first, IIOConnector second);

    public void CancelConnection();
    
    public void ConfirmConnection();
}