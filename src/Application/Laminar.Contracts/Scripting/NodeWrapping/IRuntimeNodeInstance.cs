using Laminar.Domain.Observables;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface IRuntimeNodeInstance : INotificationClient<LaminarExecutionContext>
{
    public void Update(LaminarExecutionContext context);
}