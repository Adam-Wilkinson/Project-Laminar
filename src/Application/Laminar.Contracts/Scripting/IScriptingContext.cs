using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting;

public interface IScriptingContext : IDisposable
{
    public IScript HostScript { get; }
    
    public INotificationClient<LaminarExecutionContext> UserChangedValueNotificationClient { get; }
    
    public INodeCollection Nodes { get; }
}