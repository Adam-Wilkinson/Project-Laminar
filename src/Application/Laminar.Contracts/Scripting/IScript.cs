using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Value;
using Point = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Contracts.Scripting;

public interface IScript : IEncodableDataOwner<IPersistentDictionary>
{
    public IRuntimeHost Host { get; }
    
    public INodeTree NodeTree { get; }

    IObservableValue<Point> Pan { get; }
    
    IObservableValue<double> Zoom { get; }
}
