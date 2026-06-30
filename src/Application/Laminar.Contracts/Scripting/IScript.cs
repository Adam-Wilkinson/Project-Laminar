using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Value;
using Point = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Contracts.Scripting;

public interface IScript : IEncodableDataOwner<IPersistentDictionary>
{
    public INodeTree WritableNodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }

    IObservableValue<Point> Pan { get; }
    
    IObservableValue<double> Zoom { get; }
}
