using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IRegisteredPlugin
{
    string PluginName { get; }

    string PluginDescription { get; }
    
    public bool ContainsNode(INode node);

    void Load();
    
    void Unload();
}
