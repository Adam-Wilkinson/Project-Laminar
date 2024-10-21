using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IRegisteredPlugin
{
    string PluginName { get; }

    string PluginDescription { get; }

    public IReadOnlyDictionary<string, Type> RegisteredNodes { get; }

    public bool ContainsNode(INode node);

    void Load();

    void RegisterNode(INode node);

    void Unload();
}
