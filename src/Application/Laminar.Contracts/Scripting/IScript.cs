using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Value;
using Laminar.PluginFramework.NodeSystem;
using Point = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Contracts.Scripting;

public interface IScript : IEncodableDataOwner<IPersistentDictionary>, INotificationClient<LaminarExecutionContext>
{
    public INodeTree NodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }

    IObservableValue<Point> Pan { get; }
    
    IObservableValue<double> Zoom { get; }
}
