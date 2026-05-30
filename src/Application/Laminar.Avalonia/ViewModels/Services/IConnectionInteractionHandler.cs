using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IConnectionInteractionHandler
{
    /// <summary>
    /// Resolves the connector that is the true target of an interaction with a given connector
    /// </summary>
    /// <param name="connector">The connector that was interacted with</param>
    /// <returns>The connector that is being targeted</returns>
    public IConnector? StartConnectionFrom(IConnector connector);
    
    public bool HoverConnection(IConnector first, IConnector second);

    public void CancelConnection();
    
    public void ConfirmConnection();
}