using Laminar.Core.ScriptEditor.Connections;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar_PluginFramework.Registration;

namespace Laminar.Core.PluginManagement;

public static class StaticRegistrations
{
    public static void Register(IPluginHost host)
    {
        host.RegisterInputConnector<IValueInput, ValueInputConnector>();
        host.RegisterOutputConnector<IValueOutput, ValueOutputConnector>();
    }
}
