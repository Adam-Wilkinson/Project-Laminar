using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IRuntimeNodeInstance : INotificationClient<LaminarExecutionContext>, IDisposable
{
    public INotificationClient<LaminarExecutionContext>? UserChangedValueNotificationClient { get; set; }
    
    public INode CoreNode { get; }
    
    public void Update(LaminarExecutionContext context);
}