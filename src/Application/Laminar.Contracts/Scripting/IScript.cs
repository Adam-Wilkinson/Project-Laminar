using System.ComponentModel;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Contracts.Workspaces;
using Laminar.Domain.Observables.Value;
using Point = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Contracts.Scripting;

public interface IScript : IEncodableDataOwner<IPersistentDictionary>, INotifyPropertyChanged
{
    public IWorkspace Host { get; }
    
    public IUserActionScope ActionScope { get; }
    
    public INodeGraph NodeGraph { get; }

    IObservableValue<Point> Pan { get; }
    
    IObservableValue<double> Zoom { get; }
}
