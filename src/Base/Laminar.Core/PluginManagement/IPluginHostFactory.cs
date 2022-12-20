namespace Laminar.Core.PluginManagement;

public interface IPluginHostFactory
{
    public PluginHost GetPluginhost(IRegisteredPlugin registeredPlugin);
}
