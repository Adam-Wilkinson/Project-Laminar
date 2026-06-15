namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRegistry
{
    public IRegisteredPlugin GetPluginFromName(string pluginName);
}